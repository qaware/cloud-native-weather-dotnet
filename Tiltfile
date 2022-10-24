# -*- mode: Python -*-
# allow_k8s_contexts('rancher-desktop')
# version_settings(constraint='>=0.22.2')

# to disable push with rancher desktop we need to use custom_build instead of docker_build
# docker_build('cloud-native-weather-dotnet', './DotnetWeather/', dockerfile='./DotnetWeather/Dockerfile')
custom_build('cloud-native-weather-dotnet', 'docker build -t $EXPECTED_REF ./DotnetWeather/', ['./DotnetWeather/'], disable_push=True)

k8s_yaml(kustomize('DotnetWeather/k8s/overlays/dev'))
k8s_resource(workload='weather-service', port_forwards=[port_forward(18080, 8080, 'HTTP API')], labels=['Dotnet'])
k8s_resource(workload='weather-database', port_forwards=[port_forward(11433, 1433, 'SQL API')])