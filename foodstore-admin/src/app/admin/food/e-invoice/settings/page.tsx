"use client"

import * as React from "react"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Switch } from "@/components/ui/switch"
import { eInvoiceService } from "@/lib/services/e-invoice-service"
import type { EInvoiceProvider, EInvoiceSetting, UpdateEInvoiceSettingDto } from "@/lib/types"

export default function EInvoiceSettingsPage() {
  const [providers, setProviders] = React.useState<EInvoiceProvider[]>([])
  const [settings, setSettings] = React.useState<EInvoiceSetting | null>(null)
  const [loading, setLoading] = React.useState(true)
  const [saving, setSaving] = React.useState(false)

  const [defaultProviderId, setDefaultProviderId] = React.useState("")
  const [autoIssue, setAutoIssue] = React.useState(false)
  const [defaultTemplateCode, setDefaultTemplateCode] = React.useState("")
  const [defaultSerialNumber, setDefaultSerialNumber] = React.useState("")

  React.useEffect(() => {
    Promise.all([
      eInvoiceService.getAllProviders(),
      eInvoiceService.getSettings(),
    ]).then(([providersList, settingsData]) => {
      setProviders(providersList)
      if (settingsData) {
        setSettings(settingsData)
        setDefaultProviderId(settingsData.defaultProviderId ?? "")
        setAutoIssue(settingsData.autoIssue)
        setDefaultTemplateCode(settingsData.defaultTemplateCode ?? "")
        setDefaultSerialNumber(settingsData.defaultSerialNumber ?? "")
      }
    }).catch(() => toast.error("Failed to load settings"))
    .finally(() => setLoading(false))
  }, [])

  const handleSave = async () => {
    setSaving(true)
    try {
      const data: UpdateEInvoiceSettingDto = {
        defaultProviderId: defaultProviderId || undefined,
        autoIssue,
        defaultTemplateCode: defaultTemplateCode.trim() || undefined,
        defaultSerialNumber: defaultSerialNumber.trim() || undefined,
      }
      await eInvoiceService.updateSettings(data)
      toast.success("Settings saved successfully")
    } catch (e) { toast.error((e as Error).message) }
    finally { setSaving(false) }
  }

  if (loading) return (
    <div className="flex items-center justify-center h-64">
      <div className="size-8 animate-spin rounded-full border-4 border-muted border-t-primary" />
    </div>
  )

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>E-Invoice Settings</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0 max-w-xl">
        <div className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="provider">Default Provider</Label>
            <select id="provider" className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-hidden focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              value={defaultProviderId} onChange={(e) => setDefaultProviderId(e.target.value)}>
              <option value="">Select provider...</option>
              {providers.filter(p => p.isActive).map(p => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </div>
          <div className="space-y-2">
            <Label htmlFor="templateCode">Invoice Template Code</Label>
            <Input id="templateCode" value={defaultTemplateCode} onChange={(e) => setDefaultTemplateCode(e.target.value)} placeholder="e.g. 1GTKT0/001" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="serialNumber">Invoice Serial Number</Label>
            <Input id="serialNumber" value={defaultSerialNumber} onChange={(e) => setDefaultSerialNumber(e.target.value)} placeholder="e.g. AA/22E" />
          </div>
          <div className="flex items-center gap-2">
            <Switch id="autoIssue" checked={autoIssue} onCheckedChange={setAutoIssue} />
            <Label htmlFor="autoIssue">Automatically issue invoice upon payment</Label>
          </div>
          <Button onClick={handleSave} disabled={saving}>{saving ? "Saving..." : "Save Settings"}</Button>
        </div>
      </div>
    </>
  )
}
