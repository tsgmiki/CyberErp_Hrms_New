import {
  ArrowLeftRight, Award, Banknote, BarChart3, BookOpen, BookOpenCheck, Boxes, Briefcase,
  BriefcaseBusiness, Building, Building2, CalendarCheck, CalendarClock, CalendarCog,
  CalendarDays, CalendarRange, Circle, ClipboardCheck, ClipboardList, ClipboardPlus, ClipboardType, Coins,
  DoorOpen, FilePlus2, FileHeart, FileSignature, FileText, Gauge, Gavel, GitBranch, GitBranchPlus, GitPullRequestArrow,
  Goal, GraduationCap, Grid3x3, HandCoins, Handshake, HeartHandshake, HeartPulse, KeyRound, Landmark, Layers,
  LayoutDashboard, LayoutGrid, Lightbulb, ListChecks, ListPlus, ListTree, MapPin, Medal, Megaphone,
  MessageSquareWarning, Network, Newspaper, Package, PanelsTopLeft, Plane, Receipt, Rocket, Route, Scale, ScrollText,
  Shapes, ShieldAlert, ShieldCheck, ShieldPlus, SlidersHorizontal, Sparkles, Star, Stethoscope, Tags, Target, ThumbsUp,
  TrendingUp, Trophy, UserCheck, UserCog, UserPlus, UserRoundCog, UserX, Users, UsersRound, Vote, Wallet,
  type LucideIcon,
} from "lucide-react";

/**
 * Icon names stored in Core.Module / Core.Operation resolve here (lucide-react names).
 * An explicit map — NOT `import { icons }` — keeps the whole lucide catalog out of the
 * bundle; unknown names fall back to a neutral Circle.
 */
const LUCIDE_ICONS: Record<string, LucideIcon> = {
  ArrowLeftRight, Award, Banknote, BarChart3, BookOpen, BookOpenCheck, Boxes, Briefcase,
  BriefcaseBusiness, Building, Building2, CalendarCheck, CalendarClock, CalendarCog,
  CalendarDays, CalendarRange, Circle, ClipboardCheck, ClipboardList, ClipboardPlus, ClipboardType, Coins,
  DoorOpen, FilePlus2, FileHeart, FileSignature, FileText, Gauge, Gavel, GitBranch, GitBranchPlus, GitPullRequestArrow,
  Goal, GraduationCap, Grid3x3, HandCoins, Handshake, HeartHandshake, HeartPulse, KeyRound, Landmark, Layers,
  LayoutDashboard, LayoutGrid, Lightbulb, ListChecks, ListPlus, ListTree, MapPin, Medal, Megaphone,
  MessageSquareWarning, Network, Newspaper, Package, PanelsTopLeft, Plane, Receipt, Rocket, Route, Scale, ScrollText,
  Shapes, ShieldAlert, ShieldCheck, ShieldPlus, SlidersHorizontal, Sparkles, Star, Stethoscope, Tags, Target, ThumbsUp,
  TrendingUp, Trophy, UserCheck, UserCog, UserPlus, UserRoundCog, UserX, Users, UsersRound, Vote, Wallet,
};

export function resolveNavIcon(name?: string | null): LucideIcon {
  if (!name) return Circle;
  return LUCIDE_ICONS[name] ?? Circle;
}

/** Names offered by the module/operation admin forms' icon dropdown. */
export const NAV_ICON_NAMES = Object.keys(LUCIDE_ICONS).sort();
