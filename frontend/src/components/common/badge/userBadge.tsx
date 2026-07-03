import AvatarDisplay, { type AvatarDisplayType } from "../avatar/avatarDisplay";

export interface UserBadgeProps {
  name?: string;
  email?: string;
  phone?: string;
  position?: string;
  displayType?: AvatarDisplayType;
  size?: "sm" | "md" | "lg";
}

function UserBadge({
  name,
  email,
  phone,
  position,
  displayType = "name",
  size = "sm",
}: UserBadgeProps) {
  return (
    <AvatarDisplay
      name={name}
      email={email}
      phone={phone}
      position={position}
      displayType={displayType}
      size={size}
    />
  );
}

export default UserBadge;
