import i18n from '../i18n';

export const getUserLocale = (): string => {
  return i18n.language;
};

export const setUserLocale = (locale: string): void => {
  i18n.changeLanguage(locale);
};