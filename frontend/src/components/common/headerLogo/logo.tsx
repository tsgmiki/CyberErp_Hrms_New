import logo from "@/assets/logo.jpg";

interface LogoProps {
  size?: number;
  className?: string;
}

function Logo({ size = 50, className = "" }: LogoProps) {
  return (
    <img
      className={`mx-auto ${className}`}
      src={logo}
      width={size}
      height={size}
      alt="Logo"
    />
  );
}

export default Logo;
