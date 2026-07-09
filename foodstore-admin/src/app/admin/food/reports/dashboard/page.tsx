"use client"

import * as React from "react"
import type { ColumnDef } from "@tanstack/react-table"
import { Download, RefreshCw } from "lucide-react"
import { toast } from "sonner"
import { DataTable } from "@/components/data-table"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { financialReportService } from "@/lib/services/financial-report-service"
import { organizationService } from "@/lib/services/organization-service"
import type { BranchFinancialSummary, FinancialReport, Organization } from "@/lib/types"
import { formatCurrency } from "@/lib/utils"

const dateValue = (days: number) => { const date = new Date(); date.setDate(date.getDate() + days); return date.toISOString().slice(0, 10) }

export default function FinancialReportsPage() {
  const [organizations, setOrganizations] = React.useState<Organization[]>([])
  const [organizationId, setOrganizationId] = React.useState("")
  const [branchId, setBranchId] = React.useState("")
  const [fromDate, setFromDate] = React.useState(dateValue(-29))
  const [toDate, setToDate] = React.useState(dateValue(0))
  const [report, setReport] = React.useState<FinancialReport | null>(null)
  const [loading, setLoading] = React.useState(false)
  React.useEffect(() => { organizationService.list().then(items => { setOrganizations(items); setOrganizationId(items[0]?.id || ""); setBranchId(items[0]?.branches.find(branch => branch.isActive)?.id || "") }).catch(() => toast.error("Failed to load organizations")) }, [])
  const organization = organizations.find(item => item.id === organizationId)
  async function load() { if (!organizationId) return; setLoading(true); try { setReport(await financialReportService.get(organizationId, fromDate, toDate, branchId || undefined)) } catch (error) { toast.error((error as Error).message) } finally { setLoading(false) } }
  const columns: ColumnDef<BranchFinancialSummary>[] = [
    { accessorKey: "branchName", header: "Branch" }, { accessorKey: "orders", header: "Orders" },
    { id: "conversion", header: "Conversion", cell: ({ row }) => `${row.original.paymentConversionRate}%` },
    { id: "gross", header: "Gross sales", cell: ({ row }) => formatCurrency(row.original.grossSales) },
    { id: "refunds", header: "Refunds", cell: ({ row }) => formatCurrency(row.original.refunds) },
    { id: "net", header: "Net sales", cell: ({ row }) => formatCurrency(row.original.netSales) },
    { id: "wallet", header: "Wallet", cell: ({ row }) => formatCurrency(row.original.walletPosition) },
  ]
  const totalCards = report ? [["Gross sales", report.totals.grossSales], ["Net sales", report.totals.netSales], ["Wallet position", report.totals.walletPosition], ["Pending settlement", report.totals.pendingSettlement]] : []
  return <>
    <header className="flex h-16 shrink-0 items-center gap-2"><div className="flex items-center gap-2 px-4"><SidebarTrigger className="-ml-1" /><Separator orientation="vertical" className="mr-2 h-4" /><Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Financial reports</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb></div></header>
    <div className="flex flex-1 flex-col gap-4 p-4 pt-0"><div className="flex flex-wrap items-end gap-3"><div className="grid gap-2"><Label>Organization</Label><select value={organizationId} onChange={event => { setOrganizationId(event.target.value); setBranchId("") }} className="h-9 min-w-56 rounded-md border bg-background px-3 text-sm">{organizations.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</select></div><div className="grid gap-2"><Label>Branch scope</Label><select value={branchId} onChange={event => setBranchId(event.target.value)} className="h-9 min-w-52 rounded-md border bg-background px-3 text-sm"><option value="">All authorized branches</option>{organization?.branches.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</select></div><div className="grid gap-2"><Label htmlFor="from">From (Kenyan date)</Label><input id="from" type="date" value={fromDate} onChange={event => setFromDate(event.target.value)} className="h-9 rounded-md border bg-background px-3 text-sm" /></div><div className="grid gap-2"><Label htmlFor="to">To (Kenyan date)</Label><input id="to" type="date" value={toDate} onChange={event => setToDate(event.target.value)} className="h-9 rounded-md border bg-background px-3 text-sm" /></div><Button onClick={() => void load()} disabled={loading || !organizationId}><RefreshCw className={loading ? "size-4 animate-spin" : "size-4"} /> Run report</Button><Button variant="outline" disabled={!report} onClick={() => window.open(financialReportService.exportUrl(organizationId, fromDate, toDate, branchId || undefined), "_blank", "noopener,noreferrer")}><Download className="size-4" /> CSV</Button></div>
    {report && <><div className="grid gap-4 md:grid-cols-4">{totalCards.map(([label, amount]) => <Card key={String(label)}><CardHeader className="pb-2"><CardTitle className="text-sm font-medium text-muted-foreground">{label}</CardTitle></CardHeader><CardContent className="text-2xl font-bold">{formatCurrency(amount as number)} {report.currency}</CardContent></Card>)}</div><div className="grid gap-4 md:grid-cols-3"><Card><CardHeader><CardTitle>Payment conversion</CardTitle></CardHeader><CardContent className="text-2xl font-bold">{report.totals.paymentConversionRate}%</CardContent></Card><Card><CardHeader><CardTitle>Settlements</CardTitle></CardHeader><CardContent>{report.totals.settlementCount} batches</CardContent></Card><Card><CardHeader><CardTitle>Exception queue</CardTitle></CardHeader><CardContent className="text-sm">{report.exceptions.failedPayments} failed · {report.exceptions.refundsAwaitingAction} refunds pending · {report.exceptions.settlementFailures} settlement failures · {report.exceptions.reconciliationDiscrepancies} discrepancies</CardContent></Card></div><Card><CardHeader><CardTitle>Branch performance</CardTitle></CardHeader><CardContent><DataTable columns={columns} data={report.branches} loading={loading} searchKey="branchName" searchPlaceholder="Search branches..." /></CardContent></Card></>}</div>
  </>
}
