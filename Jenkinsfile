pipeline {
  agent {
    docker {
      image 'mcr.microsoft.com/dotnet/core/sdk:3.1'
    }

  }
  stages {
    stage('Test') {
      environment {
        DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
      }
      steps {
        sh 'dotnet test Rambler.Test/Rambler.Test.csproj'
      }
    }

  }
}