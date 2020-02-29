pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/core/sdk:3.1'
        }
    }
    environment {
        DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
    }
    stages {
        stage('Build') {
            steps {
                sh 'dotnet clean'
                sh "dotnet restore"
                sh 'dotnet build'
                sh 'git fetch -t'
                script {
                  tag = sh(script: 'git describe --abbrev=0 --tags', returnStdout: true)
                  echo "Last git tag: ${tag}"
                }
                sh 'dotnet publish -c Release -r linux-x64 -o publish/linux-x64 /p:VersionSuffix=${tag}-${BUILD_NUMBER}'
                archiveArtifacts artifacts: 'publish/linux-x64/*'
            }
        }
        stage('Deploy){
            steps {
                sh 'rsync -avz -e ''ssh'' --delete publish/linux-x64/* spectro@webcam.home.lan:/data/dockerdata/rambler-dev.machaqueso.cl/www'
            }
        }
    }
}
