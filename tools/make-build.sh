#!/bin/bash

build_folder=$1
mkdir $build_folder

function build {
  single_build="$build_folder/$1"
  echo "Building $1"

  if dotnet publish ./ZombieSurvival.csproj --output $single_build -r $1 --self-contained; then
    echo "build success"
  else
    echo "build failed"
    exit 1
  fi

  # copying shaders and resources (excluding scene files)
  cp -r shaders $single_build/shaders
  cp -r resources $single_build/resources
  rm -r $single_build/resources/scenes/*
}

systems=('win-x64' 'win-x86' 'linux-x64' 'osx-x64')

for sys in "${systems[@]}"; do
  build $sys
  if [ $? -eq 0 ]; then
    printf "Successfully Built $sys\n"
  else
    echo "Failed to build $sys"
    exit 1
  fi
done