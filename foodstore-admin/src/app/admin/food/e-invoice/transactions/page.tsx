"use client"

import * as React from "react"
import { Ban, Download } from "lucide-react"
import type { ColumnDef } from "@tanstack/react-table"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { DataTable } from "@/components/data-table"
import { CrudSheet } from "@/components/crud-sheet"
import { Badge } from "@/components/ui/badge"
import { eInvoiceService } from "@/lib/services/e-invoice-service"
import { formatCurrency, formatDateTime } from "@/lib/utils"
import type { EInvoice, CancelInvoiceDto } from "@/lib/types"

const STATUS_MAP: Record<string, { label: string; variant: "default" | "secondary" | "destructive" | "outline" }> = {
  draft: { label: "Draft", variant: "secondary" },
  issued: { label: "Issued", variant: "default" },
  failed: { label: "Failed", variant: "destructive" },
  cancelled: { label: "Cancelled", variant: "outline" },
}

export default function EInvoiceTransactionsPage() {
  const [data, setData] = React.useState<EInvoice[]>([])
  const [loading, setLoading] = React.useState(true)
  const [cancelOpen, setCancelOpen] = React.useState(false)
  const [cancelTarget, setCancelTarget] = React.useState<EInvoice | null>(null)
  const [cancelReason, setCancelReason] = React.useState("")
  const [cancelling, setCancelling] = React.useState(false)

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try { const res = await eInvoiceService.getAllInvoices(); setData(res) }
    catch { toast.error("Failed to load invoices") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const openCancel = (item: EInvoice) => { setCancelTarget(item); setCancelReason(""); setCancelOpen(true) }

  const handleCancel = async () => {
    if (!cancelTarget || !cancelReason.trim()) { toast.error("Please enter a cancellation reason"); return }
    setCancelling(true)
    try {
      await eInvoiceService.cancelInvoice(cancelTarget.id, { reason: cancelReason.trim() } as CancelInvoiceDto)
      toast.success("Invoice cancelled successfully")
      setCancelOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setCancelling(false) }
  }

  const columns: ColumnDef<EInvoice>[] = [
    { id: "invoiceNumber", accessorKey: "invoiceNumber", header: "Invoice #" },
    { id: "orderCode", accessorKey: "orderCode", header: "Order #" },
    { id: "buyerName", accessorKey: "buyerName", header: "Buyer" },
    { id: "totalAmount", header: "Total", cell: ({ row }) => formatCurrency(row.original.totalAmount) },
    { id: "status", header: "Status", cell: ({ row }) => {
      const s = STATUS_MAP[row.original.status] ?? { label: row.original.status, variant: "outline" as const }
      return <Badge variant={s.variant}>{s.label}</Badge>
    }},
    { id: "createdAt", header: "Created", cell: ({ row }) => formatDateTime(row.original.createdAt) },
    { id: "actions", header: "", cell: ({ row }) => (
      <div className="flex justify-end gap-1">
        {row.original.pdfUrl && (
          <a href={row.original.pdfUrl} target="_blank" rel="noopener noreferrer" title="Download PDF"
            className="inline-flex items-center justify-center size-8 rounded-md hover:bg-muted">
            <Download className="size-4" />
          </a>
        )}
        {row.original.status === "issued" && (
          <Button variant="ghost" size="icon-sm" title="Cancel invoice" onClick={() => openCancel(row.original)}>
            <Ban className="size-4 text-destructive" />
          </Button>
        )}
      </div>
    )},
  ]

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>E-Invoice Transactions</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <DataTable columns={columns} data={data} searchKey="invoiceNumber" searchPlaceholder="Search invoices..." loading={loading} />
      </div>
      <CrudSheet open={cancelOpen} onOpenChange={setCancelOpen} title="Cancel E-Invoice"
        onSubmit={handleCancel} submitting={cancelling} submitLabel="Confirm Cancel">
        <div className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Are you sure you want to cancel invoice <strong>{cancelTarget?.invoiceNumber}</strong> for order <strong>{cancelTarget?.orderCode}</strong>?
          </p>
          <div className="space-y-2">
            <Label htmlFor="cancelReason">Cancellation Reason</Label>
            <Input id="cancelReason" value={cancelReason} onChange={(e) => setCancelReason(e.target.value)} placeholder="Enter cancellation reason..." />
          </div>
        </div>
      </CrudSheet>
    </>
  )
}
