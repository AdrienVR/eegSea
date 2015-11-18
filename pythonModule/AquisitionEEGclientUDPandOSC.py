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


if __name__ == "__main__":
    # ouverture de l'acquisition Emotiv
    headset = Emotiv(False,"",False)
    gevent.spawn(headset.setup)
    gevent.sleep(0)
    ELECTRODS=['F3','AF3','FC5','F7',
               'F4','AF4','FC6','F8',
               'T7','P7','O1',
               'T8','P8','O2'] # list of EEG electrods names
    MINQ = 0 # qualité minimale du signal exigée (1 c'est bien, avec 0 tout passe même si le casque n'est pas sur la tête)
    # ouverture de la socket UDP
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    UDP_IP = "127.0.0.1"    # adresse IP de la machine avec mer unity
    UDP_PORT = 5000         # port udp entre python et unity

    # choix de la fréquence des envois udp
    udp_freq = 128           # (Hz) fréquence de l'envoi des trames udp et osc
    buf_length = 10
    udp_period = 1./udp_freq

    inittime=time.clock()
    lasttime=inittime
    now=lasttime

    try:
        while True:
            packet = headset.dequeue()
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
                    name = str(k[1])
                    val = str(headset.sensors[k[1]]['value'])
                    if(name != '' and val != '') : mystr += name + ":" + val + ";"

                #sock.sendto(mystr,(UDP_IP,UDP_PORT))
                buf += mystr + "|"
                nbBuf += 1

                # Sortie console des infos batterie
                sys.stdout.write("\rBattery : " + str(packet.battery) + '%. ')
                if packet.battery<10 :
                    sys.stdout.write("Maybe " + str((packet.battery*600)/100) + " minutes left (LOW BATTERY).             ")
                else : sys.stdout.write("Maybe " + str((packet.battery*10)/100) + " hours left.                           ")
                lasttime=now

                lasttime=now

            if nbBuf == buf_length :
                sock.sendto(buf, (UDP_IP,UDP_PORT))
                nbBuf = 0
                buf = ""
    except KeyboardInterrupt:
        headset.close()
        sock.close()
        client.close()
    finally:
        headset.close()
        sock.close()
