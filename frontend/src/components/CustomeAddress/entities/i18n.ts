import { createI18n } from 'vue-i18n'

export const i18n = createI18n({
  locale: 'en',
  messages: {
    en: { hello: 'Hello' },
    vi: { hello: 'Xin chào' }
  }
})