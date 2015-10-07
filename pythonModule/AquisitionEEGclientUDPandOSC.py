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

# lumi�re comme speed

from emokit.emotiv import Emotiv
import socket
import gevent
import time
import numpy
import os
import OSC
import sys


if __name__ == "__main__":
    # ouverture de l'acquisition Emotiv
    headset = Emotiv(False,"",False)
    gevent.spawn(headset.setup)
    gevent.sleep(0)
    ELECTRODS=['F3','AF3','FC5','F7',
               'F4','AF4','FC6','F8',
               'T7','P7','O1',
               'T8','P8','O2'] # list of EEG electrods names
    #EEG_FREQ = 1          # fr�quense d'acquisition de l'EEG
    MINQ = 0 # qualit� minimale du signal exig�e (1 c'est bien, avec 0 tout passe m�me si le casque n'est pas sur la t�te)
    # ouverture de la socket UDP
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    UDP_IP = "127.0.0.1"    # adresse IP de la machine avec mer unity
    UDP_PORT = 5000         # port udp entre python et unity

    # choix de la fr�quence des envois udp
    udp_freq = 128           # (Hz) fr�quence de l'envoi des trames udp et osc
    udp_period = 1./udp_freq
    # initialisation des donn�es
        # trames

    inittime=time.clock()
    lasttime=inittime
    now=lasttime

    try:
        while True:
            packet = headset.dequeue()
            # maj du temps r�el pour d�cider de l'envoi par udp
            now=time.clock()
            if now-lasttime > udp_period :
                # Envoi UDP des tensions d'électrode
                mystr=str(now) + ';'
                for k in enumerate(headset.sensors) :
                    name = str(k[1])
                    val = str(headset.sensors[k[1]]['value'])
                    if(name != '' and val != '') : mystr += name + ":" + val + ";"

                sock.sendto(mystr,(UDP_IP,UDP_PORT))

                # Sortie console des infos batterie
                    #print "\rBattery", packet.battery, '% : '
                sys.stdout.write("\rBattery : " + str(packet.battery) + '%. ')
                if packet.battery<10 :
                    sys.stdout.write("Maybe " + str((packet.battery*600)/100) + " minutes left (LOW BATTERY).             ")
                else : sys.stdout.write("Maybe " + str((packet.battery*10)/100) + " hours left.                           ")
                lasttime=now
    except KeyboardInterrupt:
        headset.close()
        sock.close()
        client.close()
    finally:
        headset.close()
        sock.close()
        #client.close()
