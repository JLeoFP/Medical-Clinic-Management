import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
import { appConfig } from './app/app.config';

// Locale brasileiro: datas dd/MM/yyyy e moeda R$ 0.000,00 (DT-26).
registerLocaleData(localePt, 'pt-BR');

bootstrapApplication(App, appConfig).catch((err) => console.error(err));
