import path from 'path';
import url from 'url';

import { OpenApiGenerator } from '@goast/core';
import {
  TypeScriptAngularServicesGenerator,
  TypeScriptEasyNetworkStubsGenerator,
  TypeScriptModelsGenerator,
} from '@goast/typescript';

const __dirname = path.dirname(url.fileURLToPath(import.meta.url));

new OpenApiGenerator({ outputDir: path.join(__dirname, '../src/app/api') })
  .useType(TypeScriptModelsGenerator, { typeNameCasing: { casing: 'pascal', prefix: 'Api' } })
  .useType(TypeScriptAngularServicesGenerator, { provideKind: 'provide-fn' })
  .useType(TypeScriptEasyNetworkStubsGenerator)
  .parseAndGenerate(path.join(__dirname, '../../server/host/openapi.yml'));
