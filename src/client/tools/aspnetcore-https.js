// This script sets up HTTPS for the application using the ASP.NET Core HTTPS certificate
const { spawnSync, spawn } = require('child_process');
const path = require('path');
const fs = require('fs-extra');
const process = require('process');

if (process.env.CI !== undefined && process.env.CI.toLowerCase() === 'true') {
  console.log('Running in CI environment, skipping ASP.NET Core HTTPS certificate setup.');
  process.exit(0);
}

spawnSync('dotnet', ['dev-certs', 'https', '--trust'], { stdio: 'inherit' });

const baseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

const certificateArg = process.argv
  .map(arg => arg.match(/--name=(?<value>.+)/i))
  .filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;

if (!certificateName) {
  console.error(
    'Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.'
  );
  process.exit(-1);
}

fs.ensureDirSync(baseFolder);
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);

spawn(
  'dotnet',
  ['dev-certs', 'https', '--export-path', certFilePath, '--format', 'Pem', '--no-password'],
  { stdio: 'inherit' }
).on('exit', code => process.exit(code));
