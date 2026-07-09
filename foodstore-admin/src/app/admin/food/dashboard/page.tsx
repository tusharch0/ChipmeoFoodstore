"use client"

import * as React from "react"
import { BarChart3, DollarSign, ShoppingCart, Users } from "lucide-react"
import { toast } from "sonner"
import { Area, AreaChart, CartesianGrid, Cell, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { dashboardService } from "@/lib/services/dashboard-service"
import { signalRService } from "@/lib/services/signalr-service"
import { useAuth } from "@/lib/auth-context"
import { formatCurrency } from "@/lib/utils"
import type { DashboardStats } from "@/lib/types"

const COLORS = ["#2563eb", "#f59e0b", "#10b981", "#ef4444", "#8b5cf6", "#ec4899", "#14b8a6", "#f97316"]

export default function FoodDashboard() {
  const { user } = useAuth()
  const [stats, setStats] = React.useState<DashboardStats | null>(null)
  const [loading, setLoading] = React.useState(true)

  const loadData = React.useCallback(() => {
    dashboardService.getStats()
      .then(setStats)
      .catch(() => toast.error("Failed to load data"))
      .finally(() => setLoading(false))
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  React.useEffect(() => {
    signalRService.connect()
    const handler = () => { loadData() }
    signalRService.on("ReceiveOrderUpdate", handler)
    return () => { signalRService.off("ReceiveOrderUpdate", handler) }
  }, [loadData])

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Dashboard</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        {loading ? (
          <div className="grid gap-4 md:grid-cols-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <Card key={i}><CardHeader className="pb-2"><div className="h-4 w-24 animate-pulse rounded bg-muted" /></CardHeader><CardContent><div className="h-8 w-32 animate-pulse rounded bg-muted" /></CardContent></Card>
            ))}
          </div>
        ) : stats ? (
          <>
            <div className="grid gap-4 md:grid-cols-4">
              <Card>
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <CardTitle className="text-sm font-medium">Today's Revenue</CardTitle>
                  <DollarSign className="size-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{formatCurrency(stats.today.revenue)}</div>
                  <p className="text-xs text-muted-foreground">{stats.today.orders} orders</p>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <CardTitle className="text-sm font-medium">Monthly Revenue</CardTitle>
                  <BarChart3 className="size-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{formatCurrency(stats.month.revenue)}</div>
                  <p className="text-xs text-muted-foreground">{stats.month.orders} orders</p>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <CardTitle className="text-sm font-medium">Total Revenue</CardTitle>
                  <ShoppingCart className="size-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{formatCurrency(stats.total.revenue)}</div>
                  <p className="text-xs text-muted-foreground">Avg Order: {formatCurrency(stats.averageOrderValue)}</p>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <CardTitle className="text-sm font-medium">Customers</CardTitle>
                  <Users className="size-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{stats.totalCustomers}</div>
                  <p className="text-xs text-muted-foreground">Peak Hour: {stats.peakHour}</p>
                </CardContent>
              </Card>
            </div>

            <div className="grid gap-4 lg:grid-cols-3">
              <div className="lg:col-span-2">
                <Card>
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle>Revenue - Last 7 Days</CardTitle>
                      <div className="rounded bg-primary/10 px-2 py-0.5 text-[10px] font-bold text-primary">AI Predicted</div>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <ResponsiveContainer width="100%" height={300}>
                      <AreaChart data={stats.last7Days}>
                        <defs>
                          <linearGradient id="revenueGradient" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="hsl(var(--primary))" stopOpacity={0.3} />
                            <stop offset="95%" stopColor="hsl(var(--primary))" stopOpacity={0} />
                          </linearGradient>
                        </defs>
                        <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                        <XAxis dataKey="date" tick={{ fontSize: 12 }} tickFormatter={(v) => new Date(v).toLocaleDateString("en-US", { weekday: "short", day: "numeric" })} />
                        <YAxis tick={{ fontSize: 12 }} tickFormatter={(v) => `${(v / 1000).toFixed(0)}k`} />
                        <Tooltip formatter={(v) => formatCurrency(Number(v))} labelFormatter={(l) => new Date(l).toLocaleDateString("en-US")} />
                        <Area type="monotone" dataKey="revenue" stroke="hsl(var(--primary))" strokeWidth={2} fill="url(#revenueGradient)" />
                      </AreaChart>
                    </ResponsiveContainer>
                  </CardContent>
                </Card>
              </div>

              <div className="space-y-4">
                <Card>
                  <CardHeader className="pb-2"><CardTitle className="text-xs font-bold tracking-wider text-muted-foreground uppercase">Payments</CardTitle></CardHeader>
                  <CardContent>
                    <ResponsiveContainer width="100%" height={130}>
                      <PieChart>
                        <Pie data={Object.entries(stats.paymentMethodBreakdown).map(([k, v]) => ({ name: k === "cash" ? "Cash" : k === "bank" ? "Bank Transfer" : k, value: v }))} cx="50%" cy="50%" innerRadius={35} outerRadius={55} dataKey="value" label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}>
                          {Object.entries(stats.paymentMethodBreakdown).map((_, i) => (<Cell key={i} fill={COLORS[i % COLORS.length]} />))}
                        </Pie>
                        <Tooltip />
                      </PieChart>
                    </ResponsiveContainer>
                  </CardContent>
                </Card>
                {stats.serviceTypeStats.length > 0 && (
                  <Card>
                    <CardHeader className="pb-2"><CardTitle className="text-xs font-bold tracking-wider text-muted-foreground uppercase">Order Sources</CardTitle></CardHeader>
                    <CardContent>
                      <ResponsiveContainer width="100%" height={130}>
                        <PieChart>
                          <Pie data={stats.serviceTypeStats.map((c) => ({ name: c.name, value: c.quantity }))} cx="50%" cy="50%" innerRadius={35} outerRadius={55} dataKey="value" label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}>
                            {stats.serviceTypeStats.map((_, i) => (<Cell key={i} fill={COLORS[i % COLORS.length]} />))}
                          </Pie>
                          <Tooltip />
                        </PieChart>
                      </ResponsiveContainer>
                    </CardContent>
                  </Card>
                )}
              </div>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <Card>
                <CardHeader><CardTitle>Top Selling Items</CardTitle></CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    {stats.popularItems.slice(0, 8).map((item, i) => (
                      <div key={item.name} className="flex items-center gap-3">
                        <span className="flex size-6 shrink-0 items-center justify-center rounded-full bg-muted text-xs font-medium">{i + 1}</span>
                        <div className="flex-1 min-w-0">
                          <p className="truncate text-sm font-medium">{item.name}</p>
                          <div className="flex items-center gap-2 text-xs text-muted-foreground">
                            <span>{item.quantity} sold</span>
                            <span>•</span>
                            <span>{formatCurrency(item.revenue)}</span>
                          </div>
                        </div>
                        <div className="h-2 w-20 overflow-hidden rounded-full bg-muted">
                          <div className="h-full rounded-full bg-primary" style={{ width: `${Math.min(100, (item.quantity / Math.max(...stats.popularItems.map((p) => p.quantity))) * 100)}%` }} />
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
              <Card>
                <CardHeader><CardTitle>Popular Combos</CardTitle></CardHeader>
                <CardContent>
                  {stats.popularCombos.length > 0 ? (
                    <div className="space-y-3">
                      {stats.popularCombos.map((item, i) => (
                        <div key={item.name} className="flex items-center gap-3">
                          <span className="flex size-6 shrink-0 items-center justify-center rounded-full bg-muted text-xs font-medium">{i + 1}</span>
                          <div className="flex-1">
                            <p className="text-sm font-medium">{item.name}</p>
                            <div className="flex items-center gap-2 text-xs text-muted-foreground">
                              <span>{item.quantity} sold</span>
                              <span>•</span>
                              <span>{formatCurrency(item.revenue)}</span>
                            </div>
                          </div>
                          <div className="h-2 w-20 overflow-hidden rounded-full bg-muted">
                            <div className="h-full rounded-full bg-purple-500" style={{ width: `${Math.min(100, (item.quantity / Math.max(...stats.popularCombos.map((p) => p.quantity))) * 100)}%` }} />
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="py-8 text-center text-muted-foreground">No combo data yet</p>
                  )}
                </CardContent>
              </Card>
            </div>
          </>
        ) : (
          <div className="flex h-64 items-center justify-center text-muted-foreground">No data</div>
        )}
      </div>
    </>
  )
}