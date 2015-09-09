# -*- coding: cp1252 -*-
# AcquisitionEEGclientUDPandOSC.py
# 1 : decodes EEG raw datas
# 2 : computes spectral transform
# 3 : extract theta, alpha, beta and gamma waves energy
#     localised at 4 positions (AvG, AvD, ArG, ArD) for theta, alpha and beta waves
#     but for the whole head with 3 different frequency ranges for gamma waves
# 4 : write sensors' values, battery level, and normalised theta, alpha, beta, gamma values
# 5 : sends theta, alpha, beta and gamma values within an UDP socket
# 6 : sends normalised theta, alpha, beta and gamma values (in [0,1]) within an OSC socket

# lumière comme speed

from emokit.emotiv import Emotiv
import socket
import gevent
import time
import numpy
import os
import OSC


if __name__ == "__main__":
    # ouverture de l'acquisition Emotiv
    headset = Emotiv(False,"",False)
    gevent.spawn(headset.setup)
    gevent.sleep(0)
    ELECTRODS=['F3','AF3','FC5','F7',
               'F4','AF4','FC6','F8',
               'T7','P7','O1',
               'T8','P8','O2'] # list of EEG electrods names
    EEG_FREQ = 128          # fréquense d'acquisition de l'EEG
    MINQ = 0 # qualité minimale du signal exigée (1 c'est bien, avec 0 tout passe même si le casque n'est pas sur la tête) 
    _2pi_inv_eeg_freq = 2*numpy.pi/128
    # ouverture de la socket UDP
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    UDP_IP = "127.0.0.1"    # adresse IP de la machine avec mer unity
    UDP_PORT = 5000         # port udp entre python et unity
    # ouverture de la socket OSC
    OSC_IP = "127.0.0.1"    # adresse IP de la machine avec le son
    OSC_PORT = 5001         # port OSC entre python et le son
    client = OSC.OSCClient()
    client.connect((OSC_IP,OSC_PORT))
    alphaOSC = 1.0/30       # fréquence pour la maj des max (1.0/30 -> 60 secondes de vie)
    msg = OSC.OSCMessage()
    msg.setAddress("/test")
    # choix de la fréquence des envois udp
    udp_freq = 10           # (Hz) fréquence de l'envoi des trames udp et osc
    udp_period = 1./udp_freq
    # initialisation des données
        # trames
    datas = numpy.zeros((len(ELECTRODS),EEG_FREQ)) # tableau des trames eeg 1 seconde
    current = numpy.zeros(len(ELECTRODS)) # trame eeg en cours de traitement
    tmp=numpy.zeros(len(ELECTRODS))
    nt=0                    # numéro de colonne pour stocker la prochaine mesure eeg
        # fft
    fftRe = numpy.zeros((len(ELECTRODS),64)) # partie réelle de la fft réalisée sur chaque électrode
    fftIm = numpy.zeros((len(ELECTRODS),64)) # partie imaginaire de la fft réalisée sur chaque électrode
    fftMod2 = numpy.zeros((len(ELECTRODS),64)) # energie pour chaque électrode (module fft)
    ech_freq=numpy.zeros(64)
        # cerebral activities
    thetaWaves=numpy.zeros(4) # 4 à 8 Hz
    alphaWaves=numpy.zeros(4) # 8 à 12 Hz
    betaWaves=numpy.zeros(4) # 12 à 20 Hz
    gammaWaves=numpy.zeros(3) # 20-28, 28-36 et 36-42 Hz
    AvG=(0,4) # premier et dernier index des electrodes avant gauche
    AvD=(4,8) # ...................index des electrodes avant droite
    ArG=(8,11) # ..................index des electrodes arrière gauche
    ArD=(11,14) # .................index des electrodes arrière droite
    zones=[AvG, AvD, ArG, ArD]
    for i in range(64) :# echantillonnage log en fréquences
        ech_freq[i]=numpy.exp(numpy.log(4)+(numpy.log(42)-numpy.log(4))*i/64.)
    indexes_freq_theta=[i for i in range(64) if 4<=ech_freq[i]<8]
    indexes_freq_theta=(indexes_freq_theta[0],indexes_freq_theta[-1])
    indexes_freq_alpha=[i for i in range(64) if 8<=ech_freq[i]<=12]
    indexes_freq_alpha=(indexes_freq_alpha[0],indexes_freq_alpha[-1])
    indexes_freq_beta=[i for i in range(64) if 12<ech_freq[i]<20]
    indexes_freq_beta=(indexes_freq_beta[0],indexes_freq_beta[-1])
    indexes_freq_gamma=[]
    indexes_freq_gamma.append([i for i in range(64) if 20<=ech_freq[i]<28])
    indexes_freq_gamma[0]=(indexes_freq_gamma[0][0],indexes_freq_gamma[0][-1])
    indexes_freq_gamma.append([i for i in range(64) if 28<=ech_freq[i]<36])
    indexes_freq_gamma[1]=(indexes_freq_gamma[1][0],indexes_freq_gamma[1][-1])
    indexes_freq_gamma.append([i for i in range(64) if 36<=ech_freq[i]<43])
    indexes_freq_gamma[2]=(indexes_freq_gamma[2][0],indexes_freq_gamma[2][-1])
    inittime=time.time()
    lasttime=inittime
    now=lasttime

    # max des différentes énergies (en log)
    e_max=1e7
    e_zero=1e4
    minAsZero=numpy.log(e_zero)
    thetaMax=numpy.log([e_max,e_max,e_max,e_max])
    alphaMax=numpy.log([e_max,e_max,e_max,e_max])
    betaMax=numpy.log([e_max,e_max,e_max,e_max])
    gammaMax=numpy.log([e_max,e_max,e_max])
    
    try:
        while True:
            packet = headset.dequeue()
            gevent.sleep(0)
            # calcul de la trame current à partir des valeurs mesurées par les électrodes selon MINQ
            for i, electrod in enumerate(ELECTRODS):
                current[i]=(packet.sensors[electrod]['value']*
                            (packet.sensors[electrod]['quality']>=MINQ))
            # if nt%10 == 0 : print 'current = ', current
            # maj de la fft
            for j in range(64) :
                tmp[:]=current-datas[:,nt]
                fftRe[:,j] += (tmp*numpy.cos( (ech_freq[j]*nt)*_2pi_inv_eeg_freq ))
                fftIm[:,j] += (tmp*numpy.sin( (ech_freq[j]*nt)*_2pi_inv_eeg_freq ))            # maj de datas et de nt (tableau des trames eeg de la dernière seconde)
            datas[:,nt] = current
            nt+=1
            if nt==EEG_FREQ : # doit arriver 1 fois par seconde
                nt=0 # nt est un index circulaire comptant les trames (supposé = temps)
                os.system("cls")
                print 'At time %.1fs' % (now-inittime)
                print "Packets Received: %s Packets Processed: %s" % (headset.packets_received, headset.packets_processed)
                print('\n'.join("%s Reading: %s Quality: %s" %
                                (k[1], headset.sensors[k[1]]['value'],
                                 headset.sensors[k[1]]['quality']) for k in enumerate(headset.sensors)))
                print "\nBattery", packet.battery, '% :',
                if packet.battery<10 :
                    print "maybe", (packet.battery*600)/100, "minutes left"
                    print "WARNING EEG HEADSET HAS LOW BATTERY..."
                else : print "maybe", (packet.battery*10)/100, "hours left"
                # affichage des valeurs envoyées en OSC (dans [0,1])
                print '\nTheta; '+'; '.join('%.5g' % val for val in thetaWaves)
                print 'Alpha; '+'; '.join('%.5g' % val for val in alphaWaves)
                print 'Beta ; '+'; '.join('%.5g' % val for val in betaWaves)
                print 'Gamma; '+'; '.join('%.5g' % val for val in gammaWaves)
            # maj du temps réel pour décider de l'envoi par udp
            now=time.time()
            if now-lasttime > udp_period :
                fftMod2[:,:]=fftRe**2+fftIm**2 # compute cerebral energy for every position at every freq.
                for i in range(4) : # accumule l'énergie des différentes zones et fréquences
                    thetaWaves[i]=numpy.mean(fftMod2[zones[i][0]:zones[i][1],indexes_freq_theta[0]:indexes_freq_theta[1]])
                    alphaWaves[i]=numpy.mean(fftMod2[zones[i][0]:zones[i][1],indexes_freq_alpha[0]:indexes_freq_alpha[1]])
                    betaWaves[i]=numpy.mean(fftMod2[zones[i][0]:zones[i][1],indexes_freq_beta[0]:indexes_freq_beta[1]])
                    if i<3 : gammaWaves[i]=numpy.mean(fftMod2[:,indexes_freq_gamma[i][0]:indexes_freq_gamma[i][1]])
                # print nt, 'send udp at', now-inittime, ':'
                mystr='Theta;'+';'.join('%.5g' % val for val in thetaWaves)
                sock.sendto(mystr,(UDP_IP,UDP_PORT))
                mystr='Basse;'+';'.join('%.5g' % val for val in alphaWaves)
                sock.sendto(mystr,(UDP_IP,UDP_PORT))
                mystr='Haute;'+';'.join('%.5g' % val for val in betaWaves)
                sock.sendto(mystr,(UDP_IP,UDP_PORT))
                mystr='THF;'+';'.join('%.5g' % val for val in gammaWaves)
                sock.sendto(mystr,(UDP_IP,UDP_PORT))

                # print nt, 'send osc at', now-inittime, ':'
                # first computes values in [0,1] (0 non energy , 1 max energy)
                for i in range(4) : # O AvG, 1 AvD, 2 ArG, 3 ArD
                    if thetaWaves[i]<numpy.exp(minAsZero) : # if heaset is removed from head
                        thetaWaves[i]=numpy.exp(minAsZero)
                        thetaMax[i]=numpy.log(e_max)
                    thetaWaves[i]=numpy.log(thetaWaves[i]) # takes the log value
                    #thetaMax[i]=max((1.0-alphaOSC)*thetaMax[i]+alphaOSC*thetaWaves[i],minAsZero+3)
                    if thetaWaves[i]>thetaMax[i] : thetaMax[i]=thetaWaves[i] # update max
                    thetaWaves[i]=(thetaWaves[i]-minAsZero)/(thetaMax[i]-minAsZero) # transform value in [0,1]
                    # does the same for alpha waves
                    if alphaWaves[i]<numpy.exp(minAsZero) :
                        alphaWaves[i]=numpy.exp(minAsZero)
                        alphaMax[i]=numpy.log(e_max)
                    alphaWaves[i]=numpy.log(alphaWaves[i])
                    #alphaMax[i]=max((1.0-alphaOSC)*alphaMax[i]+alphaOSC*alphaWaves[i],minAsZero+3)
                    if alphaWaves[i]>alphaMax[i] : alphaMax[i]=alphaWaves[i]
                    alphaWaves[i]=(alphaWaves[i]-minAsZero)/(alphaMax[i]-minAsZero)
                    # does the same for beta waves
                    if betaWaves[i]<numpy.exp(minAsZero) :
                        betaWaves[i]=numpy.exp(minAsZero)
                        betaMax[i]=numpy.log(e_max)
                    betaWaves[i]=numpy.log(betaWaves[i])
                    #betaMax[i]=max((1.0-alphaOSC)*betaMax[i]+alphaOSC*betaWaves[i],minAsZero+3)
                    if betaWaves[i]>betaMax[i] : betaMax[i]=betaWaves[i]
                    betaWaves[i]=(betaWaves[i]-minAsZero)/(betaMax[i]-minAsZero)
                    if i<3 :
                        # does the same for gamma waves (only 3 different gamma waves)
                        if gammaWaves[i]<numpy.exp(minAsZero) :
                            gammaWaves[i]=numpy.exp(minAsZero)
                            gammaMax[i]=numpy.log(e_max)
                        gammaWaves[i]=numpy.log(gammaWaves[i])
                        #gammaMax[i]=max((1.0-alphaOSC)*gammaMax[i]+alphaOSC*gammaWaves[i],minAsZero+3)
                        if gammaWaves[i]>gammaMax[i] : gammaMax[i]=gammaWaves[i]
                        gammaWaves[i]=(gammaWaves[i]-minAsZero)/(gammaMax[i]-minAsZero)
                # then build the osc message to send
                msg.clearData()
                msg.append('Theta;'+';'.join('%.5g' % val for val in thetaWaves)+';')
                msg.append('Alpha;'+';'.join('%.5g' % val for val in alphaWaves)+';')
                msg.append('Beta;'+';'.join('%.5g' % val for val in betaWaves)+';')
                msg.append('Gamma;'+';'.join('%.5g' % val for val in gammaWaves))
                client.sendto(msg, (OSC_IP, OSC_PORT))
                lasttime=now
    except KeyboardInterrupt:
        headset.close()
        sock.close()
        client.close()
    finally:
        headset.close()
        sock.close()
        client.close()
