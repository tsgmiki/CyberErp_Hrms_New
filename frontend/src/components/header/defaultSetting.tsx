import { Settings } from "lucide-react";
import { useNavigate } from "react-router-dom";

function DefaultSetting() {
  const navigate = useNavigate();
  const clickHandler = () => {
    navigate("/defaultSetting");
  };

  return (
    <div className="relative inline-flex ml-3">
      <button
        value=""
        className=" bg-slate-50 text-[#4a9d9f] rounded-full border border-slate-300 p-2 "
        onClick={clickHandler}
      >
        <Settings size={20} />
      </button>
    </div>
  );
}

export default DefaultSetting;
