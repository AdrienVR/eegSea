# -*- coding: cp1252 -*-
# This is an example of popping a packet from the Emotiv class's packet queue
# and printing the gyro x and y values to the console. 

from emokit.emotiv import Emotiv
import socket
import gevent
import time
import numpy
import os

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
    MINQ = 1 # qualité minimale du signal exigée (1 c'est bien, avec 0 tout passe même si le casque n'est pas sur la tête) 
    _2pi_inv_eeg_freq = 2*numpy.pi/128
    # ouverture de la socket UDP
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    UDP_IP = "127.0.0.1"    # adresse IP de la machine avec mer unity
    UDP_PORT = 5000         # port udp entre python et unity
    udp_freq = 10           # (Hz) fréquence de l'envoi des trames udp
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
    betaWaves=numpy.zeros(4) # 12 à 24 Hz
    gammaWaves=numpy.zeros(3) # 24-33, 34-43 et 44-53 Hz
    AvG=(0,4) # premier et dernier index des electrodes avant gauche
    AvD=(4,8) # ...................index des electrodes avant droite
    ArG=(8,11) # ..................index des electrodes arrière gauche
    ArD=(11,14) # .................index des electrodes arrière droite
    zones=[AvG, AvD, ArG, ArD]
    for i in range(64) :# echantillonnage log en fréquences
        ech_freq[i]=numpy.exp(numpy.log(4)+(numpy.log(64)-numpy.log(4))*i/64.)
    indexes_freq_theta=[i for i in range(64) if 4<=ech_freq[i]<8]
    indexes_freq_theta=(indexes_freq_theta[0],indexes_freq_theta[-1])
    indexes_freq_alpha=[i for i in range(64) if 8<=ech_freq[i]<=12]
    indexes_freq_alpha=(indexes_freq_alpha[0],indexes_freq_alpha[-1])
    indexes_freq_beta=[i for i in range(64) if 12<ech_freq[i]<24]
    indexes_freq_beta=(indexes_freq_beta[0],indexes_freq_beta[-1])
    indexes_freq_gamma=[]
    indexes_freq_gamma.append([i for i in range(64) if 24<=ech_freq[i]<34])
    indexes_freq_gamma[0]=(indexes_freq_gamma[0][0],indexes_freq_gamma[0][-1])
    indexes_freq_gamma.append([i for i in range(64) if 34<=ech_freq[i]<45])
    indexes_freq_gamma[1]=(indexes_freq_gamma[1][0],indexes_freq_gamma[1][-1])
    indexes_freq_gamma.append([i for i in range(64) if 45<=ech_freq[i]<60])
    indexes_freq_gamma[2]=(indexes_freq_gamma[2][0],indexes_freq_gamma[2][-1])
    inittime=time.time()
    lasttime=inittime
    now=lasttime
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
                print '\nTheta;'+';'.join('%.5g' % val for val in thetaWaves)
                print 'Basse;'+';'.join('%.5g' % val for val in alphaWaves)
                print 'Haute;'+';'.join('%.5g' % val for val in betaWaves)
                print 'THF;'+';'.join('%.5g' % val for val in gammaWaves)
            # maj du temps réel pour décider de l'envoi par udp
            now=time.time()
            if now-lasttime > udp_period :
                fftMod2[:,:]=fftRe**2+fftIm**2 # compute cerebral energy for every position at every freq.
                for i in range(4) : # accumule l'énergie des différentes zones et fréquences
                    thetaWaves[i]=numpy.sum(fftMod2[zones[i][0]:zones[i][1],indexes_freq_theta[0]:indexes_freq_theta[1]])
                    alphaWaves[i]=numpy.sum(fftMod2[zones[i][0]:zones[i][1],indexes_freq_alpha[0]:indexes_freq_alpha[1]])
                    betaWaves[i]=numpy.sum(fftMod2[zones[i][0]:zones[i][1],indexes_freq_beta[0]:indexes_freq_beta[1]])
                    if i<3 : gammaWaves[i]=numpy.sum(fftMod2[:,indexes_freq_gamma[i][0]:indexes_freq_gamma[i][1]])
                # print nt, 'send udp at', now-inittime, ':'
                mystr='Theta;'+';'.join('%.5g' % val for val in thetaWaves)
                sock.sendto(mystr,(UDP_IP,UDP_PORT))
                mystr='Basse;'+';'.join('%.5g' % val for val in alphaWaves)
                sock.sendto(mystr,(UDP_IP,UDP_PORT))
                mystr='Haute;'+';'.join('%.5g' % val for val in betaWaves)
                sock.sendto(mystr,(UDP_IP,UDP_PORT))
                mystr='THF;'+';'.join('%.5g' % val for val in gammaWaves)
                sock.sendto(mystr,(UDP_IP,UDP_PORT))
                lasttime=now
    except KeyboardInterrupt:
        headset.close()
        sock.close()
    finally:
        headset.close()
        sock.close()
