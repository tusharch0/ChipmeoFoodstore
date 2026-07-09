"use client"

import * as React from "react"
import { Edit, Plus, Star, Trash2, Ban, BadgeCheck } from "lucide-react"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Switch } from "@/components/ui/switch"
import { NativeSelect } from "@/components/ui/native-select"
import { CrudSheet } from "@/components/crud-sheet"
import { DeleteConfirmDialog } from "@/components/confirm-dialog"
import { paymentSettingService, type Bank } from "@/lib/services/payment-setting-service"
import type { PaymentSetting, PaymentSettingCreateDto, PaymentSettingUpdateDto } from "@/lib/types"

export default function PaymentSettingsPage() {
  const [data, setData] = React.useState<PaymentSetting[]>([])
  const [banks, setBanks] = React.useState<Bank[]>([])
  const [loading, setLoading] = React.useState(true)
  const [sheetOpen, setSheetOpen] = React.useState(false)
  const [editing, setEditing] = React.useState<PaymentSetting | null>(null)
  const [submitting, setSubmitting] = React.useState(false)
  const [deleteOpen, setDeleteOpen] = React.useState(false)
  const [deleting, setDeleting] = React.useState(false)
  const [deleteTarget, setDeleteTarget] = React.useState<PaymentSetting | null>(null)

  const [formBankId, setFormBankId] = React.useState("")
  const [formBankName, setFormBankName] = React.useState("")
  const [formBankAccount, setFormBankAccount] = React.useState("")
  const [formBankAccountName, setFormBankAccountName] = React.useState("")
  const [formTemplate, setFormTemplate] = React.useState("compact2")
  const [formIsDefault, setFormIsDefault] = React.useState(false)
  const [formIsActive, setFormIsActive] = React.useState(true)

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try {
      const [settings, bks] = await Promise.all([paymentSettingService.getAll(), paymentSettingService.getBanks()])
      setData(settings); setBanks(bks)
    } catch { toast.error("Failed to load data") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const resetForm = () => {
    setFormBankId(""); setFormBankName(""); setFormBankAccount(""); setFormBankAccountName("")
    setFormTemplate("compact2"); setFormIsDefault(false); setFormIsActive(true); setEditing(null)
  }

  const openCreate = () => { resetForm(); setSheetOpen(true) }
  const openEdit = (item: PaymentSetting) => {
    setEditing(item)
    setFormBankId(item.bankId); setFormBankName(item.bankName)
    setFormBankAccount(item.bankAccount); setFormBankAccountName(item.bankAccountName ?? "")
    setFormTemplate(item.template); setFormIsDefault(item.isDefault); setFormIsActive(item.isActive)
    setSheetOpen(true)
  }

  const onBankSelect = (bin: string) => {
    const bank = banks.find((b) => b.code === bin)
    if (bank) { setFormBankId(bin); setFormBankName(bank.name) }
  }

  const handleSubmit = async () => {
    if (!formBankId || !formBankAccount || !formBankName) { toast.error("Please fill in all required fields"); return }
    setSubmitting(true)
    try {
      const base = {
        bankId: formBankId, bankAccount: formBankAccount, bankName: formBankName,
        bankAccountName: formBankAccountName || undefined, template: formTemplate,
        isDefault: formIsDefault, isActive: formIsActive,
      }
      if (editing) {
        await paymentSettingService.update(editing.id, base as PaymentSettingUpdateDto)
        toast.success("Account updated successfully")
      } else {
        await paymentSettingService.create(base as PaymentSettingCreateDto)
        toast.success("Account added successfully")
      }
      setSheetOpen(false); loadData()
    } catch (e) { toast.error((e as Error).message) }
    finally { setSubmitting(false) }
  }

  const handleSetDefault = async (id: string) => {
    try { await paymentSettingService.setDefault(id); toast.success("Set as default successfully"); loadData() }
    catch { toast.error("Failed to set default") }
  }

  const confirmDelete = (item: PaymentSetting) => { setDeleteTarget(item); setDeleteOpen(true) }
  const handleDelete = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try { await paymentSettingService.delete(deleteTarget.id); toast.success("Account deleted successfully"); setDeleteOpen(false); loadData() }
    catch { toast.error("Failed to delete account") }
    finally { setDeleting(false) }
  }

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Payment Settings</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="mb-2 flex items-center justify-between">
          <div>
            <h1 className="text-xl font-bold">Payment Accounts</h1>
            <p className="text-sm text-muted-foreground">Configure QR Code payment accounts</p>
          </div>
          <Button className="gap-1.5" onClick={openCreate}><Plus className="size-4" />Add Account</Button>
        </div>

        {loading ? (
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="h-52 animate-pulse rounded-xl bg-muted" />
            ))}
          </div>
        ) : data.length === 0 ? (
          <div className="flex flex-col items-center justify-center rounded-xl border-2 border-dashed py-16 text-muted-foreground">
            <p className="mb-4 text-lg">No payment accounts yet</p>
            <Button variant="outline" onClick={openCreate}>Add your first account</Button>
          </div>
        ) : (
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {data.map((setting) => (
              <div key={setting.id} className={`relative rounded-xl border p-6 shadow-sm transition-shadow hover:shadow-md ${setting.isDefault ? "border-primary/40 bg-primary/5 ring-1 ring-primary/30" : "bg-card"}`}>
                {setting.isDefault && (
                  <div className="absolute right-3 top-3 rounded bg-primary px-2 py-0.5 text-[10px] font-bold text-primary-foreground">DEFAULT</div>
                )}
                <div className="mb-4">
                  <h3 className="mb-3 text-lg font-bold">{setting.bankName}</h3>
                  <div className="space-y-1.5 text-sm">
                    <div className="flex justify-between border-b pb-1">
                      <span className="text-muted-foreground">Account Number:</span>
                      <span className="font-medium">{setting.bankAccount}</span>
                    </div>
                    <div className="flex justify-between border-b pb-1">
                      <span className="text-muted-foreground">Account Holder:</span>
                      <span className="font-medium uppercase">{setting.bankAccountName || "N/A"}</span>
                    </div>
                    <div className="flex justify-between border-b pb-1">
                      <span className="text-muted-foreground">Bank Code:</span>
                      <span className="font-medium">{setting.bankId}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Template:</span>
                      <span className="font-medium">{setting.template}</span>
                    </div>
                  </div>
                </div>
                <div className="mt-4 flex flex-wrap gap-2 border-t pt-4">
                  <Button variant="outline" size="sm" onClick={() => openEdit(setting)}><Edit className="mr-1 size-3.5" />Edit</Button>
                  {!setting.isDefault && (
                    <>
                      <Button variant="outline" size="sm" onClick={() => handleSetDefault(setting.id)}><Star className="mr-1 size-3.5" />Set Default</Button>
                      <Button variant="outline" size="sm" className="ml-auto text-destructive" onClick={() => confirmDelete(setting)}><Trash2 className="mr-1 size-3.5" />Delete</Button>
                    </>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <CrudSheet open={sheetOpen} onOpenChange={setSheetOpen} title={editing ? "Edit Account" : "Add New Account"} onSubmit={handleSubmit} submitting={submitting}>
        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="bankId">Bank *</Label>
            <NativeSelect className="w-full" value={formBankId} onChange={(e) => onBankSelect(e.target.value)}>
              <option value="">-- Select Bank --</option>
              {banks.map((b) => (
                <option key={b.code} value={b.code}>{b.shortName} - {b.name}</option>
              ))}
            </NativeSelect>
            <p className="text-xs text-muted-foreground">Bank name will auto-fill after selection</p>
          </div>
          <div className="space-y-2">
            <Label htmlFor="bankName">Bank Name</Label>
            <Input id="bankName" value={formBankName} readOnly className="bg-muted" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="bankAccount">Account Number *</Label>
            <Input id="bankAccount" value={formBankAccount} onChange={(e) => setFormBankAccount(e.target.value)} placeholder="e.g. 108873756885" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="bankAccountName">Account Holder</Label>
            <Input id="bankAccountName" value={formBankAccountName} onChange={(e) => setFormBankAccountName(e.target.value.toUpperCase())} placeholder="e.g. NGUYEN QUOC TUAN" className="uppercase" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="template">Template QR</Label>
            <NativeSelect className="w-full" value={formTemplate} onChange={(e) => setFormTemplate(e.target.value)}>
              <option value="compact">Compact</option>
              <option value="compact2">Compact 2</option>
              <option value="print">Print</option>
              <option value="qr_only">QR Only</option>
            </NativeSelect>
          </div>
          <div className="flex items-center gap-2">
            <Switch id="isActive" checked={formIsActive} onCheckedChange={setFormIsActive} />
            <Label htmlFor="isActive">Active</Label>
          </div>
          <div className="flex items-center gap-2">
            <Switch id="isDefault" checked={formIsDefault} onCheckedChange={setFormIsDefault} />
            <Label htmlFor="isDefault">Set as Default</Label>
          </div>
        </div>
      </CrudSheet>

      <DeleteConfirmDialog open={deleteOpen} onOpenChange={setDeleteOpen} onConfirm={handleDelete} loading={deleting} itemName={deleteTarget?.bankName} />
    </>
  )
}
