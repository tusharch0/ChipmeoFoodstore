"use client"

import * as React from "react"
import { usePathname, useRouter } from "next/navigation"

import { NavMain } from "@/components/nav-main"
import { NavUser } from "@/components/nav-user"
import { SettingsDialog } from "@/components/settings-dialog"
import { TeamSwitcher } from "@/components/team-switcher"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarRail,
} from "@/components/ui/sidebar"
import {
  GalleryVerticalEndIcon,
  FileTextIcon,
  LayoutDashboardIcon,
  ShoppingCartIcon,
  PackageIcon,
  UsersIcon,
  Settings2Icon,
  TruckIcon,
  ClipboardListIcon,
  CreditCardIcon,
  Landmark,
  PercentIcon,
  BookOpenIcon,
  SmartphoneIcon,
  TagsIcon,
  UserCogIcon,
  RadioIcon,
  Trophy,
  ReceiptTextIcon,
  Building2,
} from "lucide-react"

type Module = "food" | "employees" | "crm" | "cms"

const teams: { name: string; logo: React.ReactNode; plan: string }[] = [
  {
    name: "Restaurant Management",
    logo: <GalleryVerticalEndIcon />,
    plan: "Menu & Orders",
  },
  {
    name: "Employee Management",
    logo: <UserCogIcon />,
    plan: "Staff & Permissions",
  },
  {
    name: "Customer Management",
    logo: <UsersIcon />,
    plan: "CRM & POS",
  },
  {
    name: "Content Management",
    logo: <FileTextIcon />,
    plan: "Content & News",
  },
]

const currentModuleName: Record<Module, string> = {
  food: "Restaurant Management",
  employees: "Employee Management",
  crm: "Customer Management",
  cms: "Content Management",
}

const foodNav: {
  title: string; url?: string; icon: React.ReactNode; isActive?: boolean;
  items?: { title: string; url: string }[]
}[] = [
  {
    title: "Overview",
    icon: <LayoutDashboardIcon />,
    isActive: true,
    items: [
      { title: "Dashboard", url: "/admin/food/dashboard" },
      { title: "Analytics & AI", url: "/admin/food/analytics" },
    ],
  },
  {
    title: "Orders",
    icon: <ShoppingCartIcon />,
    items: [
      { title: "All Orders", url: "/admin/food/orders" },
      { title: "Pending", url: "/admin/food/orders?status=pending" },
    ],
  },
  {
    title: "Products",
    icon: <PackageIcon />,
    items: [
      { title: "All Products", url: "/admin/food/products" },
      { title: "Categories", url: "/admin/food/categories" },
      { title: "Toppings", url: "/admin/food/toppings" },
      { title: "Combos", url: "/admin/food/combos" },
    ],
  },
  {
    title: "Suppliers",
    url: "/admin/food/suppliers",
    icon: <TruckIcon />,
  },
  {
    title: "Order Sources",
    url: "/admin/food/sources",
    icon: <RadioIcon />,
  },
  {
    title: "Promotions",
    icon: <PercentIcon />,
    items: [
      { title: "Discount Codes", url: "/admin/food/discounts" },
    ],
  },
  {
    title: "Payment",
    icon: <CreditCardIcon />,
    items: [
      { title: "Transactions", url: "/admin/food/payments/dashboard" },
      { title: "Payment Settings", url: "/admin/food/payment-settings" },
    ],
  },
  {
    title: "E-Invoice",
    icon: <ReceiptTextIcon />,
    items: [
      { title: "Dashboard", url: "/admin/food/e-invoice/dashboard" },
      { title: "Transactions", url: "/admin/food/e-invoice/transactions" },
      { title: "Providers", url: "/admin/food/e-invoice/providers" },
      { title: "Settings", url: "/admin/food/e-invoice/settings" },
    ],
  },
  { title: "Finance", icon: <Landmark />, items: [{ title: "Wallet & Ledger", url: "/admin/food/finance/dashboard" }, { title: "Financial Reports", url: "/admin/food/reports/dashboard" }, { title: "Operations", url: "/admin/food/finance/operations" }, { title: "Settlements", url: "/admin/food/settlements/dashboard" }] },
  { title: "Branches", url: "/admin/food/organizations/dashboard", icon: <Building2 /> },
]

const employeeNav = [
  {
    title: "Overview",
    url: "/admin/employees/dashboard",
    icon: <LayoutDashboardIcon />,
    isActive: true,
  },
  {
    title: "Employees",
    icon: <UserCogIcon />,
    items: [
      { title: "All Employees", url: "/admin/employees/all" },
      { title: "Roles", url: "/admin/employees/roles" },
      { title: "Permissions", url: "/admin/employees/role-permissions" },
    ],
  },
]

const crmNav = [
  {
    title: "Overview",
    url: "/admin/crm/dashboard",
    icon: <LayoutDashboardIcon />,
    isActive: true,
  },
  {
    title: "Customers",
    url: "/admin/crm/customers",
    icon: <UsersIcon />,
  },
  {
    title: "Points Leaderboard",
    url: "/admin/crm/leaderboard",
    icon: <Trophy />,
  },
]

const cmsNav = [
  {
    title: "Overview",
    url: "/admin/cms/dashboard",
    icon: <LayoutDashboardIcon />,
    isActive: true,
  },
  {
    title: "Posts",
    url: "/admin/cms/posts",
    icon: <FileTextIcon />,
  },
  {
    title: "Tags",
    url: "/admin/cms/tags",
    icon: <TagsIcon />,
  },
  {
    title: "Categories",
    url: "/admin/cms/categories",
    icon: <BookOpenIcon />,
  },
  {
    title: "Settings",
    url: "/admin/cms/settings",
    icon: <Settings2Icon />,
  },
]

const moduleMap: Record<Module, typeof foodNav> = {
  food: foodNav,
  employees: employeeNav,
  crm: crmNav,
  cms: cmsNav,
}

function getModuleFromPath(path: string): Module {
  if (path.startsWith("/admin/employees")) return "employees"
  if (path.startsWith("/admin/crm")) return "crm"
  if (path.startsWith("/admin/cms")) return "cms"
  return "food"
}

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const pathname = usePathname()
  const router = useRouter()
  const [settingsTab, setSettingsTab] = React.useState<"profile" | "settings" | null>(null)

  const currentModule = getModuleFromPath(pathname)
  const navItems = moduleMap[currentModule]
  const activeTeam = teams.find((t) => t.name === currentModuleName[currentModule]) ?? teams[0]

  function handleTeamChange(team: { name: string; logo: React.ReactNode; plan: string }) {
    const routeMap: Record<string, Module> = {
      "Restaurant Management": "food",
      "Employee Management": "employees",
      "Customer Management": "crm",
      "Content Management": "cms",
    }
    router.push(`/admin/${routeMap[team.name] ?? "food"}`)
  }

  return (
    <>
      <Sidebar collapsible="icon" {...props}>
        <SidebarHeader>
          <TeamSwitcher teams={teams} activeTeam={activeTeam} onTeamChange={handleTeamChange} />
        </SidebarHeader>
        <SidebarContent>
          <NavMain items={navItems} />
        </SidebarContent>
        <SidebarFooter>
          <NavUser onOpenSettings={setSettingsTab} />
        </SidebarFooter>
        <SidebarRail />
      </Sidebar>
      <SettingsDialog tab={settingsTab} onClose={() => setSettingsTab(null)} />
    </>
  )
}
