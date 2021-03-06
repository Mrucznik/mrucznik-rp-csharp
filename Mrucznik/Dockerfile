#
# Build SAMP Sharp stage
#
#FROM ubuntu AS build-sampsharp
#
#WORKDIR /build
#
## install libraries
#RUN apt-get update && apt-get install -y \
#    premake4 \
#    g++ \
#    g++-multilib \
#    git \
#    make
#
## clone repository 
#RUN git clone https://github.com/ikkentim/SampSharp 
#
## build script
#RUN cd SampSharp && \
#    premake4 gmake && \
#    make config=release32
#


#
# Build gamemode stage
#
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-gamemode

WORKDIR /build

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet nuget add source http://nuget.timpotze.nl/api/v2/
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out



#
# SAMP server stage
#
FROM ubuntu

# Install libraries
RUN dpkg --add-architecture i386 && apt-get update && apt-get install -y \
    g++-multilib \ 
    wget \
    unzip \
    libicu66:i386

# Get SAMP server
RUN mkdir /home/samp && \
    wget https://sa-mp.co.id/files/samp03DLsvr_R1.tar.gz && \
    tar -xzf samp03DLsvr_R1.tar.gz -C /home/samp --strip-components=1 && \ 
    rm samp03DLsvr_R1.tar.gz
    
# Get SAMP Sharp resources
RUN wget https://github.com/ikkentim/SampSharp/releases/download/0.9.3/SampSharp-0.9.3.zip && \
    unzip SampSharp-0.9.3.zip && \
    cp -r SampSharp-0.9.3/* /home/samp && \
    rm -r SampSharp-0.9.3 && rm SampSharp-0.9.3.zip

# Get .NET Core
RUN wget https://deploy.timpotze.nl/packages/dotnet20200127.zip && \
    unzip dotnet20200127.zip && \
    cp -r dotnet20200127/runtime /home/samp/dotnet && \
    rm -r dotnet20200127 && rm dotnet20200127.zip
	
# Get crashdetect plugin
RUN wget https://github.com/Zeex/samp-plugin-crashdetect/releases/download/v4.19.4/crashdetect-4.19.4-linux.tar.gz && \
	tar -xzf crashdetect-4.19.4-linux.tar.gz -C /home/samp/plugins --wildcards --no-anchored '*.so' --strip-components=1 && \ 
    rm crashdetect-4.19.4-linux.tar.gz
	
# Get streamer plugin
RUN wget https://github.com/samp-incognito/samp-streamer-plugin/releases/download/v2.9.4/samp-streamer-plugin-2.9.4.zip && \
	unzip -j samp-streamer-plugin-2.9.4.zip plugins/streamer.so -d /home/samp/plugins && \
	rm samp-streamer-plugin-2.9.4.zip

# Change config
RUN sed -i 's/changeme/funiol/' /home/samp/server.cfg && \ 
    sed -i 's/SA-MP 0.3 Server/!PL! Mrucznik Role Play 3.0 !PL!/' /home/samp/server.cfg && \ 
    sed -i 's/gamemode0 grandlarc 1/gamemode0 empty 1/' /home/samp/server.cfg && \
    sed -i 's/filterscripts base gl_actions gl_property gl_realtime/filterscripts/' /home/samp/server.cfg && \
    echo "plugins crashdetect.so libSampSharp.so streamer.so" >> /home/samp/server.cfg && \
    echo "coreclr dotnet" >> /home/samp/server.cfg && \
    echo "gamemode gamemode/Mrucznik.dll" >> /home/samp/server.cfg

#COPY --from=build-sampsharp /build/SampSharp/bin/* /home/samp/plugins/
COPY --from=build-gamemode /build/out /home/samp/gamemode
#COPY ./bin/Release/netcoreapp3.1/* /home/samp/gamemode/ # local build

EXPOSE 7777    

WORKDIR /home/samp
ENTRYPOINT ["./samp03svr"]