"use client"

import * as React from "react"
import type { ColumnDef } from "@tanstack/react-table"
import { toast } from "sonner"
import { DataTable } from "@/components/data-table"
import { CrudSheet } from "@/components/crud-sheet"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { paymentService } from "@/lib/services/payment-service"
import type { PaymentIntent, PaymentIntentDetail } from "@/lib/types"
import { formatCurrency, formatDateTime } from "@/lib/utils"

export default function PaymentsDashboardPage() {
  const [items, setItems] = React.useState<PaymentIntent[]>([])
  const [loading, setLoading] = React.useState(true)
  const [selected, setSelected] = React.useState<PaymentIntentDetail | null>(null)
  const [open, setOpen] = React.useState(false)
  const [reason, setReason] = React.useState("")
  const [refunding, setRefunding] = React.useState(false)

  const load = React.useCallback(async () => {
    setLoading(true)
    try { setItems(await paymentService.search()) } catch { toast.error("Failed to load payment transactions") } finally { setLoading(false) }
  }, [])
  React.useEffect(() => { load() }, [load])
  const showDetail = async (item: PaymentIntent) => {
    try { setSelected(await paymentService.get(item.id)); setReason(""); setOpen(true) } catch { toast.error("Failed to load payment details") }
  }
  const refund = async () => {
    if (!selected || !reason.trim()) { toast.error("Enter a refund reason"); return }
    setRefunding(true)
    try { await paymentService.refund(selected.id, reason.trim()); toast.success("Refund requested"); setOpen(false); load() }
    catch (error) { toast.error((error as Error).message) } finally { setRefunding(false) }
  }
  const columns: ColumnDef<PaymentIntent>[] = [
    { accessorKey: "orderCode", header: "Order" },
    { accessorKey: "customerPhone", header: "Phone" },
    { accessorKey: "providerReference", header: "Provider reference" },
    { id: "amount", header: "Amount", cell: ({ row }) => `${formatCurrency(row.original.amount)} ${row.original.currency}` },
    { id: "status", header: "Status", cell: ({ row }) => <Badge variant={row.original.status === "succeeded" ? "default" : row.original.status === "failed" ? "destructive" : "secondary"}>{row.original.status}</Badge> },
    { id: "createdAt", header: "Created", cell: ({ row }) => formatDateTime(row.original.createdAt) },
    { id: "actions", header: "", cell: ({ row }) => <Button variant="ghost" size="sm" onClick={() => showDetail(row.original)}>Details</Button> },
  ]
  return <>
    <header className="flex h-16 shrink-0 items-center gap-2"><div className="flex items-center gap-2 px-4"><SidebarTrigger className="-ml-1" /><Separator orientation="vertical" className="mr-2 h-4" /><Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Payment Transactions</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb></div></header>
    <div className="flex flex-1 flex-col gap-4 p-4 pt-0"><DataTable columns={columns} data={items} searchKey="providerReference" searchPlaceholder="Search payment references..." loading={loading} /></div>
    <CrudSheet open={open} onOpenChange={setOpen} title="Payment lifecycle" onSubmit={refund} submitting={refunding} submitLabel="Request refund">
      <div className="space-y-4"><div className="grid grid-cols-2 gap-2 text-sm"><span>Status</span><Badge>{selected?.status}</Badge><span>Order</span><span>{selected?.orderCode ?? selected?.orderId}</span><span>Reference</span><span>{selected?.providerReference ?? "—"}</span></div>
      <div><Label htmlFor="refundReason">Refund reason</Label><Input id="refundReason" value={reason} onChange={(e) => setReason(e.target.value)} disabled={selected?.status !== "succeeded"} /></div>
      <div className="space-y-2"><Label>Webhook history</Label>{selected?.events.map(event => <div className="rounded border p-2 text-xs" key={event.id}><strong>{event.eventType}</strong> · {event.processingOutcome ?? "pending"}<br />{formatDateTime(event.receivedAt)} · attempts {event.attempts}</div>)}</div></div>
    </CrudSheet>
  </>
}
