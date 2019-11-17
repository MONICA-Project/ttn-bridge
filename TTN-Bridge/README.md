# Fraunhofer.Fit.IoT.TTN.Bridge (TTN-Bridge)
<!-- Short description of the project. -->

Converts Trackingdata from the Things Network mqtt to local mqtt data. This readme is meant for describing the application. 

<!-- A teaser figure may be added here. It is best to keep the figure small (<500KB) and in the same repo -->

## Getting Started
<!-- Instruction to make the project up and running. -->

The project documentation is available on the [Wiki](https://github.com/MONICA-Project/ttn-bridge/wiki).

## Deployment
<!-- Deployment/Installation instructions. If this is software library, change this section to "Usage" and give usage examples -->

This repository is only for containing the code from TTN-Bridge. If you want to develop, please goto the [Lora-Scral-Project](https://github.com/MONICA-Project/ttn-bridge-project). This repository contains all references as github submodules, even this one.

## Development
<!-- Developer instructions. -->
* Versioning: Use [SemVer](http://semver.org/) and tag the repository with full version string. E.g. `v1.0.0`

### Prerequisite
This projects depends on different librarys.

#### Linking to
##### Internal
* BlubbFish.Utils ([Utils](http://git.blubbfish.net/vs_utils/Utils))
* BlubbFish.Utils.IoT ([Utils-IoT](http://git.blubbfish.net/vs_utils/Utils-IoT))
* BlubbFish.Utils.IoT.Connector.Data.Mqtt ([ConnectorDataMqtt](http://git.blubbfish.net/vs_utils/ConnectorDataMqtt))
* BlubbFish.Utils.IoT.Bots ([Bot-Utils](http://git.blubbfish.net/vs_utils/Bot-Utils.git))

##### External
* litjson
* Mono.Posix
* M2Mqtt

## Contributing
Contributions are welcome. 

Please fork, make your changes, and submit a pull request. For major changes, please open an issue first and discuss it with the other authors.

## Affiliation
![MONICA](https://github.com/MONICA-Project/template/raw/master/monica.png)  
This work is supported by the European Commission through the [MONICA H2020 PROJECT](https://www.monica-project.eu) under grant agreement No 732350.