"use client"

import * as React from "react"
import { Building2, Pencil, Plus } from "lucide-react"
import { toast } from "sonner"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { CrudSheet } from "@/components/crud-sheet"
import { organizationService, type BranchInput } from "@/lib/services/organization-service"
import type { Organization, OrganizationBranch } from "@/lib/types"

const emptyForm: BranchInput = { name: "", code: "", address: "", city: "", phone: "", openingHoursJson: "", taxSettingsJson: "", kitchenRouting: "", isActive: true }

export default function OrganizationDashboardPage() {
  const [organizations, setOrganizations] = React.useState<Organization[]>([])
  const [organizationId, setOrganizationId] = React.useState("")
  const [branch, setBranch] = React.useState<OrganizationBranch | null>(null)
  const [form, setForm] = React.useState<BranchInput>(emptyForm)
  const [open, setOpen] = React.useState(false)
  const [saving, setSaving] = React.useState(false)
  const load = React.useCallback(async () => { try { const items = await organizationService.list(); setOrganizations(items); setOrganizationId(current => current || items[0]?.id || "") } catch { toast.error("Failed to load organizations") } }, [])
  React.useEffect(() => { void load() }, [load])
  const organization = organizations.find(item => item.id === organizationId)
  function edit(item?: OrganizationBranch) { setBranch(item ?? null); setForm(item ? { ...item } : emptyForm); setOpen(true) }
  async function submit() {
    if (!organizationId || !form.name || (!branch && !form.code)) { toast.error("A branch name and code are required"); return }
    setSaving(true)
    try { branch ? await organizationService.updateBranch(organizationId, branch.id, form) : await organizationService.addBranch(organizationId, form); toast.success(branch ? "Branch updated" : "Branch added"); setOpen(false); await load() } catch (error) { toast.error((error as Error).message) } finally { setSaving(false) }
  }
  return <>
    <header className="flex h-16 shrink-0 items-center gap-2"><div className="flex items-center gap-2 px-4"><SidebarTrigger className="-ml-1" /><Separator orientation="vertical" className="mr-2 h-4" /><Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>Branches</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb></div></header>
    <div className="flex flex-1 flex-col gap-4 p-4 pt-0"><div className="flex flex-wrap items-end justify-between gap-3"><div className="grid gap-2"><Label htmlFor="organization">Restaurant organization</Label><select id="organization" value={organizationId} onChange={event => setOrganizationId(event.target.value)} className="h-9 min-w-64 rounded-md border bg-background px-3 text-sm">{organizations.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}</select></div><Button onClick={() => edit()} disabled={!organizationId}><Plus className="size-4" /> Add branch</Button></div>
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">{organization?.branches.map(item => <Card key={item.id}><CardHeader className="flex-row items-start justify-between gap-3"><div><CardTitle className="flex items-center gap-2"><Building2 className="size-5" />{item.name}</CardTitle><p className="mt-1 text-sm text-muted-foreground">{item.code} · {item.city || "Location not set"}</p></div><Button variant="ghost" size="icon" onClick={() => edit(item)}><Pencil className="size-4" /><span className="sr-only">Edit {item.name}</span></Button></CardHeader><CardContent className="space-y-2 text-sm"><p>{item.address || "No address configured"}</p><p>Kitchen: {item.kitchenRouting || "Default routing"}</p><p className={item.isActive ? "text-emerald-600" : "text-muted-foreground"}>{item.isActive ? "Active" : "Inactive"}</p></CardContent></Card>)}</div></div>
    <CrudSheet open={open} onOpenChange={setOpen} title={branch ? "Edit branch" : "Add branch"} onSubmit={() => void submit()} submitting={saving}><div className="grid gap-4">{([ ["name", "Branch name"], ["code", "Branch code"], ["address", "Address"], ["city", "City"], ["phone", "Phone"], ["kitchenRouting", "Kitchen routing"], ["openingHoursJson", "Opening-hours JSON"], ["taxSettingsJson", "Tax-settings JSON"] ] as const).map(([key, label]) => <div className="grid gap-2" key={key}><Label htmlFor={key}>{label}</Label><Input id={key} value={form[key] ?? ""} disabled={key === "code" && !!branch} onChange={event => setForm({ ...form, [key]: event.target.value })} /></div>)}<label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={form.isActive ?? true} onChange={event => setForm({ ...form, isActive: event.target.checked })} /> Active branch</label></div></CrudSheet>
  </>
}
