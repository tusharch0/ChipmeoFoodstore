"use client"

import * as React from "react"
import { Edit, Save } from "lucide-react"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { blogSettingService } from "@/lib/services/blog-setting-service"
import type { BlogSettingDto } from "@/lib/types"

export default function BlogSettingsPage() {
  const [settings, setSettings] = React.useState<BlogSettingDto[]>([])
  const [loading, setLoading] = React.useState(true)
  const [editingKey, setEditingKey] = React.useState<string | null>(null)
  const [editValue, setEditValue] = React.useState("")
  const [editDesc, setEditDesc] = React.useState("")

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try { const res = await blogSettingService.getAll(); setSettings(res) }
    catch { toast.error("Failed to load settings") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  const startEdit = (s: BlogSettingDto) => { setEditingKey(s.key); setEditValue(s.value); setEditDesc(s.description ?? "") }
  const cancelEdit = () => { setEditingKey(null); setEditValue(""); setEditDesc("") }

  const handleSave = async (key: string) => {
    try {
      await blogSettingService.upsert(key, { value: editValue, description: editDesc || undefined })
      toast.success("Setting saved")
      cancelEdit()
      loadData()
    } catch (e) { toast.error((e as Error).message) }
  }

  const settingLabels: Record<string, string> = {
    site_name: "Site Name",
    site_description: "Site Description",
    posts_per_page: "Posts Per Page",
    default_template: "Default Template",
    facebook_url: "Facebook URL",
    twitter_url: "Twitter URL",
    instagram_url: "Instagram URL",
    footer_text: "Footer Text",
    contact_email: "Contact Email",
  }

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" /><Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb><BreadcrumbList><BreadcrumbItem><BreadcrumbPage>CMS Settings</BreadcrumbPage></BreadcrumbItem></BreadcrumbList></Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <h1 className="text-lg font-semibold">CMS Settings</h1>
        {loading ? (
          <p className="text-muted-foreground">Loading...</p>
        ) : (
          <div className="grid gap-4 max-w-2xl">
            {settings.length === 0 && <p className="text-muted-foreground">No settings yet. Add a setting by saving a new value.</p>}
            {settings.map((s) => (
              <div key={s.key} className="rounded-lg border p-4">
                <div className="flex items-center justify-between mb-2">
                  <div>
                    <Label className="font-medium">{settingLabels[s.key] ?? s.key}</Label>
                    <p className="text-xs text-muted-foreground">{s.description}</p>
                  </div>
                  <Button variant="ghost" size="sm" onClick={() => startEdit(s)}><Edit className="size-4" /></Button>
                </div>
                {editingKey === s.key ? (
                  <div className="grid gap-2">
                    <Input value={editValue} onChange={(e) => setEditValue(e.target.value)} />
                    <Textarea value={editDesc} onChange={(e) => setEditDesc(e.target.value)} placeholder="Description" rows={2} />
                    <div className="flex gap-2">
                      <Button size="sm" onClick={() => handleSave(s.key)}><Save className="size-4 mr-1" />Save</Button>
                      <Button size="sm" variant="outline" onClick={cancelEdit}>Cancel</Button>
                    </div>
                  </div>
                ) : (
                  <p className="text-sm">{s.value}</p>
                )}
              </div>
            ))}
          </div>
        )}
        <div className="max-w-2xl">
          <p className="text-xs text-muted-foreground mt-4">Tip: New settings can be added by calling the API directly. Common keys: site_name, posts_per_page, default_template, facebook_url, contact_email.</p>
        </div>
      </div>
    </>
  )
}