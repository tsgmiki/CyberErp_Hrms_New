"use client";

import { Globe } from "lucide-react";
import { startTransition } from "react";
import {  setUserLocale } from "@/services/language";
import DropDownButton from "@/components/ui/dropDownButton";

function LanguageSwitcher() {
  const menuItems = [
    {
      key: "en",
      label: "English",
    },
    {
      key: "ao",
      label: "Affan Oromo",
    },
  ];
  function onLocaleChange(item: any) {
    startTransition(() => {
      setUserLocale(item.key);
    });
  }
  return (
    <div className="relative inline-flex ml-3 ">
      <DropDownButton
        value=""
        className=" bg-[#005A9C] text-slate-50 rounded-full border-[1px] border-slate-300 p-[8px] "
        menu={menuItems}
        icon={<Globe size={20} />}
        onClick={onLocaleChange}
        disabled={false}
      />
    </div>
  );
}

export default LanguageSwitcher;
