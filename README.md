# eegSea
Casque EEG - projet La mer est ton miroir

installer python27 (ça c'est déjà fait normalement !)

* installer pip
* récuperer get-pip.py :
* https://raw.githubusercontent.com/pypa/pip/master/contrib/get-pip.py
* puis exécuter dans un terminal
cmd windows: get-pip.py
* term linux : sudo python2.7 get-pip.py

installer un compilateur visual c++ compatible python27
http://www.microsoft.com/en-us/download/details.aspx?id=44266
exécuter l'installeur VC4python27.msi (en cliquant dessus)

* installer gevent dans un terminal (cmd):
c:\Python27\Scripts\pip install gevent
* linux only : sudo pip install gevent

installer pycrypto dans un terminal (cmd):
c:\Python27\Scripts\pip install pycrypto
* linux only : sudo pip install pycrypto

installer pywinusb dans un terminal (cmd):
c:\Python27\Scripts\pip install pywinusb
* linux only : sudo pip install pywinusb

* désarchiver emokit-master.7z
* puis dans un terminal (cmd):
* se placer dans le répertoire ...\emokit-master\python avec des cd ; 
par exemple :
cd "Mes Documents"
cd MerMiroir\emokit-master\python
* puis exécuter l'intall :
windows : setup.py install
* linux only : sudo python2.7 setup.py install

* d'autre part, pour le script python acquisitionEEGclientUDPandOSC.py, 
* installer le module pyOSC et numpy
dans un terminal (cmd):
c:\Python27\Scripts\pip install pyOSC
c:\Python27\Scripts\pip install numpy
* ou l'équivalent pour linux :
* linux only : sudo pip install pyOSC
* linux only : sudo pip install numpy

s'il y a un problème non résolu avant ce stade, la suite ne peut pas fonctionner.
---
A partir de maintenant, il n'y a plus besoin de terminal (cmd).

tester emotiv seul :
pluguer la clé emotiv et allumer le casque.
s'assurer que les deux led vertes sur la clé sont allumées en continu.

dans le navigateur de fichiers aller dans ...emokit-master\python\emokit
cliquer sur emotiv.pyc (l'icone est celle de python mais un peu plus noire)
cela doit ouvrir une fenêtre dans laquelle s'affiche l'énergie des éléctrodes EEG et la qualité de la mesure.
si le casque Emotiv n'est pas sur la tête, la qualité doit être nulle.
si le casque Emotiv est sur la tête avec les électrodes et les cheveux humides, 
	la qualité de chaque électrode doit dépasser 1 (classiquement entre 5 et 12).
	Il faut insister d'abord sur les éléctrodes tamporales (celles qui ne sont pas sur des tiges en plastique)

REMARQUE IMORTANTE :
la première fois, il faut noyer les électrodes dans leur petite boite noire
et s'assurer que les tampons sont bien enfoncés.
s'il n'y a plus de liquide dans le petit flacon, il suffit de mettre de l'eau du robinet.
puis placer les 14 électrodes sur le casque.
Cette opération étant longue et délicate, par la suite il vaut mieux imbiber les électrodes directement sur le casque.
En effet, il nous est arrivé de désouder le fil qui arrive sous l'électrode lors de cette opération de montage/démontage.
Par contre, lorsque l'électrode est en place, il faut éviter de mouiller la partie bluetouth du casque ;)
Il faut s'arrêter d'ajouter un goutte dès que le liseret plastique/tampon mouille :
l'électrode est alors saturée en eau et une goutte de plus ferait déborder.
si un tampon tombe lors de la manipulation du casque, il suffit de le remettre en place (ça arrive).

Il ne reste plus qu'à lancer AcquisitionEEGcleintUDPandOSC.py (en cliquant dessus) :
il y a une variable : MINQ définie à la ligne 36 (clic droit ouvrir avec idle)
qui définit la valeur minimale de qualité nécessaire pour tenir compte de la mesure des électrodes.
à 0 : toute valeur est prise en compte (même si le casque n'est pas sur la tête)
à 1 : il faut que le casque soit sur la tête et bien placé pour avoir des valeurs non nulles.
Cliquer sur AcquisitionEEGclientUDPandOSC.py ouvre un fenêtre 
et chaque seconde les valeurs des capteurs et des trames OSC y sont affichées.
Cette fenêtre aide à vérifier que le casque est bien positionné.

Du côté Mer (unity) ça devrait marcher sans rien changer.
Dites moi ce que vous aimeriez : je prévois de travailler au moins 1 jour par semaine sur ce projet pour améliorer les choses.

PS : openvibe ne sert plus à rien.
