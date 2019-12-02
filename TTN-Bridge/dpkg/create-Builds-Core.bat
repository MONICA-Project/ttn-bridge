mkdir ..\..\..\Builds
bash.exe -c "./make-deb-core.sh amd64"
bash.exe -c "./make-deb-core.sh armhf"
pause