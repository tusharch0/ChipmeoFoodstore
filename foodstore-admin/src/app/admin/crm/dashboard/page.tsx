"use client"

import * as React from "react"
import { Users, ShoppingBag, DollarSign, TrendingUp, Cake, Gift } from "lucide-react"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { dashboardService } from "@/lib/services/dashboard-service"
import { customerService } from "@/lib/services/customer-service"
import { formatCurrency, formatDate } from "@/lib/utils"
import type { UpcomingBirthdays } from "@/lib/types"

export default function CrmDashboard() {
  const [totalCustomers, setTotalCustomers] = React.useState<number | null>(null)
  const [todayOrders, setTodayOrders] = React.useState<number | null>(null)
  const [todayRevenue, setTodayRevenue] = React.useState<number | null>(null)
  const [totalRevenue, setTotalRevenue] = React.useState<number | null>(null)
  const [birthdays, setBirthdays] = React.useState<UpcomingBirthdays | null>(null)

  React.useEffect(() => {
    (async () => {
      try {
        const [stats, bdays] = await Promise.all([
          dashboardService.getStats(),
          customerService.getUpcomingBirthdays(),
        ])
        setTotalCustomers(stats.totalCustomers)
        setTodayOrders(stats.today.orders)
        setTodayRevenue(stats.today.revenue)
        setTotalRevenue(stats.total.revenue)
        setBirthdays(bdays)
      } catch {
        toast.error("Failed to load stats")
      }
    })()
  }, [])

  const cards = [
    { label: "Total Customers", value: totalCustomers !== null ? totalCustomers.toLocaleString() : "…", icon: Users },
    { label: "Today's Orders", value: todayOrders !== null ? todayOrders.toLocaleString() : "…", icon: ShoppingBag },
    { label: "Today's Revenue", value: todayRevenue !== null ? formatCurrency(todayRevenue) : "…", icon: DollarSign },
    { label: "Total Revenue", value: totalRevenue !== null ? formatCurrency(totalRevenue) : "…", icon: TrendingUp },
  ]

  const levelBadge = (level?: string) => {
    if (!level) return null
    const colors: Record<string, string> = {
      bronze: "bg-amber-100 text-amber-800",
      silver: "bg-slate-200 text-slate-800",
      gold: "bg-yellow-100 text-yellow-800",
      platinum: "bg-indigo-100 text-indigo-800",
      diamond: "bg-blue-100 text-blue-800",
    }
    return <span className={`rounded px-1.5 py-0.5 text-xs font-medium ${colors[level] || "bg-gray-100"}`}>{level}</span>
  }

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Customer Overview</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="grid auto-rows-min gap-4 md:grid-cols-4">
          {cards.map((card) => (
            <div key={card.label} className="flex items-center gap-4 rounded-xl border p-4">
              <div className="flex size-12 items-center justify-center rounded-lg bg-primary/10">
                <card.icon className="size-6 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold">{card.value}</p>
                <p className="text-sm text-muted-foreground">{card.label}</p>
              </div>
            </div>
          ))}
        </div>

        <div className="rounded-xl border p-4">
          <div className="flex items-center gap-2 mb-3">
            <Cake className="size-5 text-pink-500" />
            <h2 className="font-semibold">Customer Birthdays</h2>
            {birthdays && birthdays.totalThisWeek > 0 && (
              <Badge className="ml-auto">{birthdays.totalThisWeek} this week</Badge>
            )}
          </div>
          {!birthdays ? (
            <p className="text-sm text-muted-foreground">Loading...</p>
          ) : birthdays.thisWeek.length === 0 ? (
            <p className="text-sm text-muted-foreground">No customer birthdays this week</p>
          ) : (
            <div className="space-y-2">
              {birthdays.thisWeek.map((c) => (
                <div key={c.id} className="flex items-center justify-between rounded-lg border p-3">
                  <div className="flex items-center gap-3">
                    <Gift className="size-4 text-pink-400" />
                    <div>
                      <p className="text-sm font-medium">{c.name}</p>
                      <p className="text-xs text-muted-foreground">{c.customerCode} {c.phone ? `• ${c.phone}` : ""}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    {levelBadge(c.membershipLevel)}
                    <span className="text-xs text-muted-foreground">{c.birthday ? formatDate(c.birthday) : "—"}</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </>
  )
}
