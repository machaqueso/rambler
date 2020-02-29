pipeline {
  agent {
    docker {
      image 'mcr.microsoft.com/dotnet/core/sdk:3.1'
    }

  }
  stages {
    stage('') {
      steps {
        sh 'dotnet test Rambler.Test/Rambler.Test.csproj'
      }
    }

  }
}