// Get different languages according to the key
import zhCN from 'vxe-table/lib/locale/lang/zh-CN'
import enUS from 'vxe-table/lib/locale/lang/en-US'
import zhTW from 'vxe-table/lib/locale/lang/zh-TW'
import { store } from '@/store'
import i18n from '../i18n'

import en from '../langsJson/en.json'
import cn from '../langsJson/cn.json'
import tw from '../langsJson/tw.json'
import ptbr from '../langsJson/pt_BR.json'

export function getSelectedLang(lang: string) {
  switch (lang) {
    case 'zh':
      return 'zh_CN'
    case 'en':
      return 'en_US'
    case 'tw':
      return 'zh_TW'
    case 'pt':
      return 'pt_BR'
    default:
      return 'en_US'
  }
}

// Obtain default language pack based on language
export function getLangPackage(lang: string) {
  const result = { zh_CN: {}, zh_TW: {}, en_US: {}, pt_BR: {} }
  switch (lang) {
    case 'zh_CN':
      result.zh_CN = { ...cn, ...zhCN }
      break
    case 'zh_TW':
      result.zh_TW = { ...tw, ...zhTW }
      break
    case 'pt_BR':
      // vxe-table has no pt-BR locale pack in this version; fall back to en-US for its own grid strings
      result.pt_BR = { ...ptbr, ...enUS }
      break
    case 'en_US':
    default:
      result.en_US = { ...en, ...enUS }
      break
  }
  return result
}

// Because it is lazy to load the language pack, the user will reload the language pack after switching languages
export function installNewLang() {
  let lang = store.getters['system/language']
  lang = getSelectedLang(lang)
  const messages = getLangPackage(lang)
  if (lang && messages) {
    // Use setLocaleMessage to add or update language packs
    i18n.global.setLocaleMessage(lang, messages[lang])
    return
  }
  // Default English
  i18n.global.setLocaleMessage('en_US', { ...en, ...enUS })
}
