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
                sh 'git log --oneline'
                script {
                  GIT_TAG = sh(returnStdout: true, script: 'git describe --abbrev=0 --tags')
                }
                sh 'dotnet publish -c Release -r linux-x64 -o publish/linux-x64 /p:VersionSuffix=${GIT_TAG}-${BUILD_NUMBER}'
                archiveArtifacts artifacts: 'publish/linux-x64/*'
            }
        }
    }
}