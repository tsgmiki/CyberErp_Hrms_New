import { useTranslation } from "react-i18next"


export default function AppName() {
  const {t} = useTranslation()
  return (
    <>
      {' '}
      <span
        className={
          ' ml-4 mt-3 text-lg align-middle items-center font-semibold text-[#fbfdff]'
        }
      >
        {t('CompanyName')}{' '}
      </span>
    </>
  )
}
