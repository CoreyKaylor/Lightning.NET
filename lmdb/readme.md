### Compiling multi-platform targets for LMDB

This process has historically been very tedious and error prone due to manual nature taken in the past.
Now all targets can be handled in a single script.

If you deal with this kind of stuff frequently and know a better way, please contribute.

This assumes you're working from a Mac.

Install mingw64 and android-ndk build tools for targeting Windows from a Mac

`brew bundle install`

`export NDK="/opt/homebrew/share/android-ndk"`

If you already use and license docker desktop, great the docker process will likely work as-is.
I switched to using colima w/docker-cli. This setup will work using that approach.

```bash
brew install docker
brew install colima
colima start --vm-type=vz --vz-rosetta #use macos built-in virtualization
docker run --privileged --rm tonistiigi/binfmt --install all #this will install all available platforms
# Ready to build now and should have all necessary platforms available for docker
```

Docker has been used to deal with the various linux targets and the script assumes
you have a version of docker that supports multi-platform runs with --platform.