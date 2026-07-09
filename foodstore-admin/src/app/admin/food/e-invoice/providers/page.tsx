"use client"

import * as React from "react"
import { Edit, Plus, Trash2, Plug, Wifi } from "lucide-react"
import type { ColumnDef } from "@tanstack/react-table"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Switch } from "@/components/ui/switch"
import { DataTable } from "@/components/data-table"
import { CrudSheet } from "@/components/crud-sheet"
import { DeleteConfirmDialog } from "@/components/confirm-dialog"
import { StatusBadge } from "@/components/status-badge"
import { eInvoiceService } from "@/lib/services/e-invoice-service"
import type { EInvoiceProvider, CreateEInvoiceProviderDto, UpdateEInvoiceProviderDto } from "@/lib/types"

const PROVIDER_TYPES = [
  { value: "misa", label: "MISA meInvoice" },
  { value: "viettel", label: "Viettel S-Invoice" },
  { value: "bkav", label: "BKAV" },
  { value: "fpt", label: "FPT eInvoice" },
  { value: "easyinvoice", label: "EasyInvoice" },
]

export default function EInvoiceProvidersPage() {
  const [data, setData] = React.useState<EInvoiceProvider[]>([])
  const [loading, setLoading] = React.useState(true)
  const [sheetOpen, setSheetOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<EInvoiceProvider | null>(null)
  const [submitting, setSubmitting] = React.useState(false)
  const [deleteOpen, setDeleteOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  const [deleteTarget, setDeleteTarget] = React.useState<EInvoiceProvider | null>(null)
  const [testingId, setTestingId] = React.useState<string | null>(null)

  const [formName, setFormName] = React.useState("")
  const [formProviderType, setFormProviderType] = React.useState("misa")
  const [formIsActive, setFormIsActive] = React.useState(true)
  const [formDescription, setFormDescription] = React.useState("")
  const [formConfig, setFormConfig] = React.useState("")

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try { const res = await eInvoiceService.getAllProviders(); setData(res) }
    catch { toast.error("Failed to load e-invoice providers") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const resetForm = () => {
    setFormName(""); setFormProviderType("misa"); setFormIsActive(true)
    setFormDescription(""); setFormConfig(""); setEditing(null)
  }

  const openCreate = () => { resetForm(); setSheetOpen(true) }

  const openEdit = (item: EInvoiceProvider) => {
    setEditing(item)
    setFormName(item.name)
    setFormProviderType(item.providerType)
    setFormIsActive(item.isActive)
    setFormDescription(item.description ?? "")
    setFormConfig(JSON.stringify(item.config, null, 2))
    setSheetOpen(true)
  }

  const handleTestConnection = async (id: string) => {
    setTestingId(id)
    try {
      const ok = await eInvoiceService.testProviderConnection(id)
      toast.success(ok ? "Connection successful!" : "Connection failed")
    } catch { toast.error("Failed to test connection") }
    finally { setTestingId(null) }
  }

  const handleSubmit = async () => {
    if (!formName.trim()) { toast.error("Please enter a provider name"); return }
    let parsedConfig: Record<string, unknown> = {}
    if (formConfig.trim()) {
      try { parsedConfig = JSON.parse(formConfig) }
      catch { toast.error("Invalid JSON config"); return }
    }
    setSubmitting(true)
    try {
      if (editing) {
        await eInvoiceService.updateProvider(editing.id, {
          name: formName.trim(), providerType: formProviderType,
          isActive: formIsActive, config: parsedConfig,
          description: formDescription.trim() || undefined,
        } as UpdateEInvoiceProviderDto)
        toast.success("Provider updated successfully")
      } else {
        await eInvoiceService.createProvider({
          name: formName.trim(), providerType: formProviderType,
          isActive: formIsActive, config: parsedConfig,
          description: formDescription.trim() || undefined,
        } as CreateEInvoiceProviderDto)
        toast.success("Provider added successfully")
      }
      setSheetOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setSubmitting(false) }
  }

  const confirmDelete = (item: EInvoiceProvider) => { setDeleteTarget(item); setDeleteOpen(true) }
  const handleDelete = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await eInvoiceService.deleteProvider(deleteTarget.id)
      toast.success("Provider deleted successfully")
      setDeleteOpen(false); loadData()
    } catch { toast.error("Failed to delete provider") }
    finally { setDeleting(false) }
  }

  const columns: ColumnDef<EInvoiceProvider>[] = [
    { id: "name", accessorKey: "name", header: "Provider Name" },
    { id: "providerType", header: "Type", cell: ({ row }) => {
      const t = PROVIDER_TYPES.find(p => p.value === row.original.providerType)
      return t?.label ?? row.original.providerType
    }},
    { id: "isActive", header: "Status", cell: ({ row }) => <StatusBadge status={row.original.isActive} /> },
    { id: "actions", header: "", cell: ({ row }) => (
      <div className="flex justify-end gap-1">
        <Button variant="ghost" size="icon-sm" title="Test connection"
          onClick={() => handleTestConnection(row.original.id)} disabled={testingId === row.original.id}>
          <Wifi className={`size-4 ${testingId === row.original.id ? "animate-pulse" : ""}`} />
        </Button>
        <Button variant="ghost" size="icon-sm" onClick={() => openEdit(row.original)}><Edit className="size-4" /></Button>
        <Button variant="ghost" size="icon-sm" onClick={() => confirmDelete(row.original)}><Trash2 className="size-4 text-destructive" /></Button>
      </div>
    )},
  ]

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>E-Invoice Providers</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <DataTable columns={columns} data={data} searchKey="name" searchPlaceholder="Search providers..." loading={loading}
          toolbarActions={<Button className="gap-1.5" onClick={openCreate}><Plus className="size-4" />Add Provider</Button>} />
      </div>
      <CrudSheet open={sheetOpen} onOpenChange={setSheetOpen} title={editing ? "Edit Provider" : "Add Provider"} onSubmit={handleSubmit} submitting={submitting}>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Provider Name</Label>
            <Input id="name" value={formName} onChange={(e) => setFormName(e.target.value)} placeholder="e.g. MISA meInvoice" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="providerType">Type</Label>
            <select id="providerType" className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-hidden focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              value={formProviderType} onChange={(e) => setFormProviderType(e.target.value)}>
              {PROVIDER_TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
            </select>
          </div>
          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Input id="description" value={formDescription} onChange={(e) => setFormDescription(e.target.value)} placeholder="Notes about the provider..." />
          </div>
          <div className="space-y-2">
            <Label htmlFor="config">Config (JSON)</Label>
            <textarea id="config" className="flex min-h-24 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-hidden focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 font-mono"
              value={formConfig} onChange={(e) => setFormConfig(e.target.value)}
              placeholder='{"baseUrl": "https://api.misa.meinvoice.vn", "clientId": "...", "clientSecret": "..."}' />
          </div>
          <div className="flex items-center gap-2">
            <Switch id="isActive" checked={formIsActive} onCheckedChange={setFormIsActive} />
            <Label htmlFor="isActive">Active</Label>
          </div>
        </div>
      </CrudSheet>
      <DeleteConfirmDialog open={deleteOpen} onOpenChange={setDeleteOpen} onConfirm={handleDelete} loading={deleting} itemName={deleteTarget?.name} />
    </>
  )
}
