"use client"

import * as React from "react"
import { Eye, Loader2 } from "lucide-react"
import type { ColumnDef } from "@tanstack/react-table"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog"
import { DataTable } from "@/components/data-table"
import { StatusBadge } from "@/components/status-badge"
import { orderService } from "@/lib/services/order-service"
import { signalRService } from "@/lib/services/signalr-service"
import { formatCurrency, formatDateTime } from "@/lib/utils"
import type { Order } from "@/lib/types"

const STATUS_FLOW: Record<string, string[]> = {
  pending: ["confirmed", "cancelled"],
  confirmed: ["preparing", "cancelled"],
  preparing: ["ready", "cancelled"],
  ready: ["served"],
  served: ["paid"],
  paid: [],
  cancelled: [],
}

const STATUS_LABELS: Record<string, string> = {
  pending: "Pending", confirmed: "Confirmed", preparing: "Preparing",
  ready: "Ready", served: "Served", paid: "Paid", cancelled: "Cancelled",
}

export default function OrdersPage() {
  const [data, setData] = React.useState<Order[]>([])
  const [loading, setLoading] = React.useState(true)
  const [page, setPage] = React.useState(1)
  const pageSize = 10
  const [totalCount, setTotalCount] = React.useState(0)
  const [fromDate, setFromDate] = React.useState("")
  const [toDate, setToDate] = React.useState("")
  const [detailOpen, setDetailOpen] = React.useState(false)
  const [selectedOrder, setSelectedOrder] = React.useState<Order | null>(null)
  const [statusUpdating, setStatusUpdating] = React.useState<string | null>(null)

  const loadData = React.useCallback(async (p?: number) => {
    setLoading(true)
    try {
      const res = await orderService.getPaged(p ?? page, pageSize, fromDate || undefined, toDate || undefined)
      setData(res.items); setTotalCount(res.totalCount)
    } catch { toast.error("Failed to load orders") }
    finally { setLoading(false) }
  }, [page, pageSize, fromDate, toDate])

  React.useEffect(() => { loadData() }, [loadData])

  React.useEffect(() => {
    const handler = () => { loadData() }
    signalRService.connect().then(() => { signalRService.on("ReceiveOrderUpdate", handler) })
    return () => { signalRService.off("ReceiveOrderUpdate", handler) }
  }, [loadData])

  const handleStatusUpdate = async (orderId: string, newStatus: string) => {
    setStatusUpdating(orderId)
    try {
      await orderService.updateStatus(orderId, newStatus)
      toast.success(`Status changed to: ${STATUS_LABELS[newStatus] ?? newStatus}`)
      loadData()
      if (selectedOrder?.id === orderId) { const updated = await orderService.getById(orderId); setSelectedOrder(updated) }
    } catch { toast.error("Failed to update status") }
    finally { setStatusUpdating(null) }
  }

  const handleSetUnpaid = async (orderId: string) => {
    setStatusUpdating(orderId)
    try { await orderService.setUnpaid(orderId); toast.success("Marked as unpaid"); loadData() }
    catch { toast.error("Failed to change status") }
    finally { setStatusUpdating(null) }
  }

  const viewDetail = async (order: Order) => {
    try { const detail = await orderService.getById(order.id); setSelectedOrder(detail); setDetailOpen(true) }
    catch { toast.error("Failed to load order details") }
  }

  const columns: ColumnDef<Order>[] = [
    { id: "orderCode", accessorKey: "orderCode", header: "Order #" },
    { id: "customerName", header: "Customer", cell: ({ row }) => row.original.customerName ?? row.original.customerPhone ?? "—" },
    { id: "totalAmount", header: "Total", cell: ({ row }) => formatCurrency(row.original.totalAmount) },
    { id: "status", header: "Status", cell: ({ row }) => <StatusBadge status={row.original.status} customLabels={STATUS_LABELS} /> },
    { id: "sourceName", header: "Source", cell: ({ row }) => row.original.sourceName ?? "—" },
    { id: "createdAt", header: "Created", cell: ({ row }) => formatDateTime(row.original.createdAt) },
    { id: "actions", header: "", cell: ({ row }) => (
      <Button variant="ghost" size="icon-sm" onClick={() => viewDetail(row.original)}><Eye className="size-4" /></Button>
    )},
  ]

  const menuItemNames = selectedOrder?.items?.map((item) => {
    const name = item.menuItemName ?? item.comboName ?? `Item #${item.menuItemId ?? item.comboId}`
    const addons = item.addons?.map((a) => `${a.addonName ?? "Topping"} x${a.quantity}`).join(", ")
    return { name, quantity: item.quantity, unitPrice: item.unitPrice, total: (item.totalPrice ?? item.unitPrice * item.quantity), addons }
  }) ?? []

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Orders</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            <Label htmlFor="fromDate" className="text-sm">From</Label>
            <Input id="fromDate" type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} className="w-40" />
          </div>
          <div className="flex items-center gap-2">
            <Label htmlFor="toDate" className="text-sm">To</Label>
            <Input id="toDate" type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} className="w-40" />
          </div>
          <Button variant="outline" size="sm" onClick={() => loadData()}>Filter</Button>
          {(fromDate || toDate) && (
            <Button variant="ghost" size="sm" onClick={() => { setFromDate(""); setToDate("") }}>Clear Filters</Button>
          )}
        </div>
        <DataTable columns={columns} data={data} searchKey="orderCode" searchPlaceholder="Search orders..." loading={loading} />
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>Total: {totalCount} orders</span>
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => { setPage((p) => p - 1); loadData(page - 1) }}>
              Previous Page
            </Button>
            <span>Page {page}</span>
            <Button variant="outline" size="sm" disabled={page * pageSize >= totalCount} onClick={() => { setPage((p) => p + 1); loadData(page + 1) }}>
              Next Page
            </Button>
          </div>
        </div>
      </div>

      <Dialog open={detailOpen} onOpenChange={setDetailOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Order {selectedOrder?.orderCode}</DialogTitle>
            <DialogDescription>
              {selectedOrder && formatDateTime(selectedOrder.createdAt)} — {selectedOrder?.sourceName && `Source: ${selectedOrder.sourceName}`}
            </DialogDescription>
          </DialogHeader>
          {selectedOrder && (
            <div className="space-y-6">
              <div className="flex items-center justify-between">
                <StatusBadge status={selectedOrder.status} customLabels={STATUS_LABELS} />
                <div className="text-right">
                  <div className="text-sm text-muted-foreground">Customer</div>
                  <div className="font-medium">{selectedOrder.customerName ?? selectedOrder.customerPhone ?? "Walk-in"}</div>
                </div>
              </div>
              <div className="space-y-2">
                <h4 className="text-sm font-medium">Ordered Items</h4>
                <div className="rounded-lg border">
                  {menuItemNames.map((item, i) => (
                    <div key={i} className="flex items-center justify-between border-b p-3 last:border-0">
                      <div>
                        <div className="text-sm font-medium">{item.name} x{item.quantity}</div>
                        {item.addons && <div className="text-xs text-muted-foreground">{item.addons}</div>}
                      </div>
                      <div className="text-sm">{formatCurrency(item.total)}</div>
                    </div>
                  ))}
                </div>
              </div>
              <div className="space-y-1 text-right">
                {selectedOrder.discountAmount > 0 && (
                  <div className="text-sm text-muted-foreground">
                    Discount: {selectedOrder.discountCode && `(${selectedOrder.discountCode}) `}-{formatCurrency(selectedOrder.discountAmount)}
                  </div>
                )}
                <div className="text-lg font-bold">Total: {formatCurrency(selectedOrder.totalAmount)}</div>
              </div>
              {selectedOrder.history && selectedOrder.history.length > 0 && (
                <div className="space-y-2">
                  <h4 className="text-sm font-medium">Status History</h4>
                  <div className="space-y-2">
                    {selectedOrder.history.map((h) => (
                      <div key={h.id} className="flex items-center gap-2 text-sm">
                        <div className="size-2 rounded-full bg-primary" />
                        <span className="text-muted-foreground">{formatDateTime(h.changedAt)}</span>
                        <span>{STATUS_LABELS[h.toStatus] ?? h.toStatus}</span>
                        {h.changedByName && <span className="text-xs text-muted-foreground">— {h.changedByName}</span>}
                      </div>
                    ))}
                  </div>
                </div>
              )}
              <div className="space-y-2">
                <h4 className="text-sm font-medium">Update Status</h4>
                <div className="flex flex-wrap gap-2">
                  {(STATUS_FLOW[selectedOrder.status] ?? []).map((nextStatus) => (
                    <Button key={nextStatus} size="sm" variant={nextStatus === "cancelled" ? "destructive" : "default"}
                      onClick={() => handleStatusUpdate(selectedOrder.id, nextStatus)} disabled={statusUpdating === selectedOrder.id}>
                      {statusUpdating === selectedOrder.id && <Loader2 className="mr-1 size-3 animate-spin" />}
                      {STATUS_LABELS[nextStatus] ?? nextStatus}
                    </Button>
                  ))}
                  {selectedOrder.status === "paid" && (
                    <Button size="sm" variant="outline" onClick={() => handleSetUnpaid(selectedOrder.id)} disabled={statusUpdating === selectedOrder.id}>
                      Mark as Unpaid
                    </Button>
                  )}
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </>
  )
}
