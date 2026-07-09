"use client"

import * as React from "react"
import { BarChart3, ShoppingCart, Receipt, Wand2 } from "lucide-react"
import { toast } from "sonner"
import { Bar, BarChart, CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { NativeSelect } from "@/components/ui/native-select"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { dashboardService } from "@/lib/services/dashboard-service"
import { formatCurrency } from "@/lib/utils"
import type { Analytics, Forecast, Recommendation } from "@/lib/types"

export default function AnalyticsPage() {
  const [dateRange, setDateRange] = React.useState("7days")
  const [fromDate, setFromDate] = React.useState("")
  const [toDate, setToDate] = React.useState("")
  const [analytics, setAnalytics] = React.useState<Analytics | null>(null)
  const [forecast, setForecast] = React.useState<Forecast | null>(null)
  const [recommendations, setRecommendations] = React.useState<Recommendation[]>([])
  const [loading, setLoading] = React.useState(true)

  const updateDatesFromRange = (range: string) => {
    const now = new Date()
    const to = now.toISOString().split("T")[0]
    let from: string
    switch (range) {
      case "7days": from = new Date(now.getTime() - 7 * 86400000).toISOString().split("T")[0]; break
      case "30days": from = new Date(now.getTime() - 30 * 86400000).toISOString().split("T")[0]; break
      case "thisMonth": from = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split("T")[0]; break
      case "last3Months": from = new Date(now.getTime() - 90 * 86400000).toISOString().split("T")[0]; break
      default: from = ""
    }
    setFromDate(from); setToDate(to)
  }

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try {
      const [a, f, r] = await Promise.all([
        dashboardService.getAnalytics(fromDate || undefined, toDate || undefined, "day").catch(() => null),
        dashboardService.getForecast(7).catch(() => null),
        dashboardService.getRecommendations().catch(() => []),
      ])
      setAnalytics(a); setForecast(f); setRecommendations(r)
    } catch { toast.error("Failed to load data") }
    finally { setLoading(false) }
  }, [fromDate, toDate])

  React.useEffect(() => { updateDatesFromRange(dateRange) }, [dateRange])
  React.useEffect(() => { if (fromDate || dateRange !== "custom") loadData() }, [fromDate, dateRange, loadData])

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Analytics & Forecast</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="flex items-center gap-3 rounded-xl border bg-card p-2 shadow-sm">
          <NativeSelect className="w-fit border-0 bg-transparent font-medium" value={dateRange} onChange={(e) => { const v = e.target.value; setDateRange(v); if (v !== "custom") updateDatesFromRange(v) }}>
            <option value="7days">Last 7 days</option>
            <option value="30days">Last 30 days</option>
            <option value="thisMonth">This month</option>
            <option value="last3Months">Last 3 months</option>
            <option value="custom">Custom</option>
          </NativeSelect>
          {dateRange === "custom" && (
            <div className="flex items-center gap-2 border-l pl-3">
              <Input type="date" className="h-8 w-fit" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
              <span className="text-muted-foreground">→</span>
              <Input type="date" className="h-8 w-fit" value={toDate} onChange={(e) => setToDate(e.target.value)} />
              <Button size="sm" onClick={loadData}>Filter</Button>
            </div>
          )}
        </div>

        {loading ? (
          <div className="grid gap-4 md:grid-cols-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <Card key={i}><CardHeader className="pb-2"><div className="h-4 w-28 animate-pulse rounded bg-muted" /></CardHeader><CardContent><div className="h-8 w-36 animate-pulse rounded bg-muted" /></CardContent></Card>
            ))}
          </div>
        ) : analytics ? (
          <>
            <div className="grid gap-4 md:grid-cols-3">
              <Card>
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <CardTitle className="text-sm font-medium">Total Revenue</CardTitle>
                  <BarChart3 className="size-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold text-indigo-600">{formatCurrency(analytics.totalRevenue)}</div>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <CardTitle className="text-sm font-medium">VAT Tax</CardTitle>
                  <Receipt className="size-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold text-emerald-600">{formatCurrency(analytics.totalVat)}</div>
                </CardContent>
              </Card>
              <Card>
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <CardTitle className="text-sm font-medium">Total Orders</CardTitle>
                  <ShoppingCart className="size-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold text-orange-600">{analytics.totalOrders}</div>
                </CardContent>
              </Card>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <Card>
                <CardHeader><CardTitle>Revenue Trend</CardTitle></CardHeader>
                <CardContent>
                  <ResponsiveContainer width="100%" height={280}>
                    <BarChart data={analytics.revenueChart}>
                      <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                      <XAxis dataKey="label" tick={{ fontSize: 11 }} />
                      <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `${(v / 1000).toFixed(0)}k`} />
                      <Tooltip formatter={(v) => formatCurrency(Number(v))} />
                      <Bar dataKey="value" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
                    </BarChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
              <Card>
                <CardHeader><CardTitle>Order Count</CardTitle></CardHeader>
                <CardContent>
                  <ResponsiveContainer width="100%" height={280}>
                    <BarChart data={analytics.ordersChart}>
                      <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                      <XAxis dataKey="label" tick={{ fontSize: 11 }} />
                      <YAxis tick={{ fontSize: 11 }} />
                      <Tooltip />
                      <Bar dataKey="value" fill="#10b981" radius={[4, 4, 0, 0]} />
                    </BarChart>
                  </ResponsiveContainer>
                </CardContent>
              </Card>
            </div>

            {forecast && forecast.forecasts.length > 0 && (
              <Card className="border-indigo-200 bg-gradient-to-br from-indigo-50/50 to-background">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className="rounded-lg bg-indigo-600 p-2">
                        <Wand2 className="size-5 text-white" />
                      </div>
                      <div>
                        <CardTitle>Revenue Forecast (AI Powered)</CardTitle>
                        <CardDescription>Machine Learning predicts the next 7 days</CardDescription>
                      </div>
                    </div>
                    <div className="rounded border border-indigo-200 bg-card px-2 py-1 font-mono text-xs text-indigo-700">SSA-Forecasting</div>
                  </div>
                </CardHeader>
                <CardContent>
                  <ResponsiveContainer width="100%" height={280}>
                    <LineChart data={forecast.forecasts}>
                      <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                      <XAxis dataKey="date" tick={{ fontSize: 11 }} tickFormatter={(v) => new Date(v).toLocaleDateString("en-US", { day: "numeric", month: "numeric" })} />
                      <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `${(v / 1000).toFixed(0)}k`} />
                      <Tooltip formatter={(v) => formatCurrency(Number(v))} labelFormatter={(l) => new Date(l).toLocaleDateString("en-US")} />
                      <Line type="monotone" dataKey="revenue" stroke="#6366f1" strokeWidth={2} dot={{ r: 3, fill: "#6366f1" }} />
                    </LineChart>
                  </ResponsiveContainer>
                  <p className="mt-2 text-center text-xs italic text-muted-foreground">* Forecast is for reference only, based on historical data.</p>
                </CardContent>
              </Card>
            )}

            <div className="grid gap-4 md:grid-cols-2">
              <Card>
                <CardHeader><CardTitle>Top Selling Items</CardTitle></CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    {analytics.topItems.map((item, i) => (
                      <div key={item.name} className="flex items-center gap-3">
                        <span className="flex size-6 shrink-0 items-center justify-center rounded-full bg-muted text-xs font-medium">{i + 1}</span>
                        <div className="flex-1">
                          <p className="text-sm font-medium">{item.name}</p>
                          <div className="h-2 w-full overflow-hidden rounded-full bg-muted">
                            <div className="h-full rounded-full bg-primary" style={{ width: `${Math.min(100, (item.sold / Math.max(...analytics.topItems.map((t) => t.sold))) * 100)}%` }} />
                          </div>
                        </div>
                        <span className="text-sm font-medium">{item.sold}</span>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
              <Card>
                <CardHeader><CardTitle>Popular Combos</CardTitle></CardHeader>
                <CardContent>
                  {analytics.popularCombos.length > 0 ? (
                    <div className="space-y-3">
                      {analytics.popularCombos.map((item, i) => (
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
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="py-8 text-center text-muted-foreground">No combo data yet</p>
                  )}
                </CardContent>
              </Card>
            </div>

            {recommendations.length > 0 && (
              <Card className="border-purple-200 bg-gradient-to-br from-purple-50/50 to-background">
                <CardHeader>
                  <div className="flex items-center gap-3">
                    <div className="rounded-lg bg-purple-600 p-2">
                      <Wand2 className="size-5 text-white" />
                    </div>
                    <div>
                      <CardTitle>Combo Suggestions (AI Recommendation)</CardTitle>
                      <CardDescription>Items frequently ordered together - Combo opportunities</CardDescription>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {recommendations.map((rec, i) => (
                      <div key={i} className="rounded-lg border border-purple-100 bg-card p-4 shadow-sm">
                        <div className="mb-3 flex items-center gap-2">
                          <span className="rounded bg-muted px-2 py-1 text-xs font-medium">{rec.item1Name}</span>
                          <span className="text-muted-foreground">+</span>
                          <span className="rounded bg-muted px-2 py-1 text-xs font-medium">{rec.item2Name}</span>
                        </div>
                        <div className="mb-2 flex items-end justify-between">
                          <div>
                            <p className="text-xs text-muted-foreground line-through">{formatCurrency(rec.totalOriginalPrice)}</p>
                            <p className="text-lg font-bold text-purple-600">{formatCurrency(rec.suggestedPrice)}</p>
                          </div>
                          <span className="rounded-full bg-green-50 px-2 py-1 text-xs font-medium text-green-600">Save 10%</span>
                        </div>
                        <p className="mt-2 border-t pt-2 text-xs text-muted-foreground">{rec.reason}</p>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            )}
          </>
        ) : (
          <div className="flex h-64 items-center justify-center text-muted-foreground">No data</div>
        )}
      </div>
    </>
  )
}