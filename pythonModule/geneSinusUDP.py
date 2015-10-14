# -*- coding: cp1252 -*-
# AcquisitionEEGclientUDPandOSC.py

from emokit.emotiv import Emotiv
import socket
import gevent
import time
import numpy
import os
import OSC
import sys
import math


if __name__ == "__main__":
    # qualité minimale du signal exigée (1 c'est bien, avec 0 tout passe même si le casque n'est pas sur la tête)
    MINQ = 0

    #Paramétrage du sinus (fréquence en Hz)
    freqSin = 5
    tSin = 1.0/freqSin
    timeSin = 0.0
    sinVal = 0.0

    # ouverture de la socket UDP
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    UDP_IP = "127.0.0.1"    # adresse IP de la machine avec mer unity
    UDP_PORT = 5000         # port udp entre python et unity

    # choix de la fréquence des envois udp
    udp_freq = 128           # (Hz) fréquence de l'envoi des trames udp et osc
    udp_period = 1./udp_freq

    inittime=time.clock()
    lasttime=inittime
    now=lasttime

    buf = ""
    nbBuf = 0

    try:
        while True:
            # maj du temps réel pour décider de l'envoi par udp
            now=time.clock()


            if now-lasttime > udp_period :
                # Envoi UDP des tensions d'électrode
                timeSin += udp_period
                if timeSin < tSin :
                    sinVal = math.sin(2.0*math.pi*timeSin/tSin)
                else :
                    timeSin = 0
                    sinVal = 0

                mystr=str(now) + ';'
                for k in range(1, 5) :
                    name = "e" + str(k)
                    val = str(sinVal)
                    if(name != '' and val != '') : mystr += name + ":" + val + ";"

                #sock.sendto(mystr,(UDP_IP,UDP_PORT))
                buf += mystr + "|"
                nbBuf += 1

                lasttime=now

            if nbBuf == 10 :
                sock.sendto(buf, (UDP_IP,UDP_PORT))
                nbBuf = 0
                buf = ""

    except KeyboardInterrupt:
        sock.close()
        client.close()
    finally:
        sock.close()
