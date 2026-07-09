"use client"

import * as React from "react"
import { Check, Loader2, Save } from "lucide-react"
import { toast } from "sonner"

import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage } from "@/components/ui/breadcrumb"
import { Button } from "@/components/ui/button"
import { Separator } from "@/components/ui/separator"
import { SidebarTrigger } from "@/components/ui/sidebar"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Badge } from "@/components/ui/badge"
import { permissionService } from "@/lib/services/permission-service"
import { roleService } from "@/lib/services/role-service"
import type { Permission, Role } from "@/lib/types"

const TEMPLATES: Record<string, string[]> = {
  Admin: ["*"],
  Cashier: ["order.view", "order.create", "order.edit", "payment.process"],
  Kitchen: ["order.view", "order.status.update"],
}

export default function RolePermissionsPage() {
  const [roles, setRoles] = React.useState<Role[]>([])
  const [permissions, setPermissions] = React.useState<Permission[]>([])
  const [selectedRoleId, setSelectedRoleId] = React.useState<string | null>(null)
  const [rolePermissionCodes, setRolePermissionCodes] = React.useState<Set<string>>(new Set())
  const [loading, setLoading] = React.useState(true)
  const [saving, setSaving] = React.useState(false)

  const loadData = React.useCallback(async () => {
    setLoading(true)
    try {
      const [rls, perms] = await Promise.all([roleService.getAll(), permissionService.getAll()])
      setRoles(rls.filter((r) => r.isActive))
      setPermissions(perms)
    } catch { toast.error("Failed to load data") }
    finally { setLoading(false) }
  }, [])

  React.useEffect(() => { loadData() }, [loadData])

  React.useEffect(() => {
    if (!selectedRoleId) return
    permissionService.getRolePermissions(selectedRoleId).then((codes) => {
      setRolePermissionCodes(new Set(codes))
    }).catch(() => toast.error("Failed to load role permissions"))
  }, [selectedRoleId])

  const grouped = React.useMemo(() => {
    const map: Record<string, Permission[]> = {}
    permissions.forEach((p) => {
      if (!map[p.module]) map[p.module] = []
      map[p.module].push(p)
    })
    return map
  }, [permissions])

  const togglePermission = (code: string) => {
    setRolePermissionCodes((prev) => {
      const next = new Set(prev)
      if (next.has(code)) next.delete(code)
      else next.add(code)
      return next
    })
  }

  const toggleModule = (modulePerms: Permission[], checked: boolean) => {
    setRolePermissionCodes((prev) => {
      const next = new Set(prev)
      modulePerms.forEach((p) => {
        if (checked) next.add(p.code)
        else next.delete(p.code)
      })
      return next
    })
  }

  const applyTemplate = (templateName: string) => {
    const pattern = TEMPLATES[templateName]
    if (!pattern) return
    const isWildcard = pattern[0] === "*"
    if (isWildcard) {
      setRolePermissionCodes(new Set(permissions.map((p) => p.code)))
    } else {
      const matching = permissions.filter((p) => pattern.includes(p.code))
      setRolePermissionCodes(new Set(matching.map((p) => p.code)))
    }
  }

  const handleSave = async () => {
    if (!selectedRoleId) return
    setSaving(true)
    try {
      await permissionService.updateRolePermissions(selectedRoleId, Array.from(rolePermissionCodes))
      toast.success("Permissions updated successfully")
    } catch { toast.error("Failed to update permissions") }
    finally { setSaving(false) }
  }

  const protectedRoles = ["root", "customer"]
  const selectedRole = roles.find((r) => r.id === selectedRoleId)
  const isProtected = selectedRole ? protectedRoles.includes(selectedRole.name.toLowerCase()) : false

  return (
    <>
      <header className="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
        <div className="flex items-center gap-2 px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <Breadcrumb>
            <BreadcrumbList>
              <BreadcrumbItem><BreadcrumbPage>Role Permissions</BreadcrumbPage></BreadcrumbItem>
            </BreadcrumbList>
          </Breadcrumb>
        </div>
      </header>
      <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        {loading ? (
          <div className="flex flex-1 items-center justify-center">
            <Loader2 className="size-8 animate-spin text-muted-foreground" />
          </div>
        ) : (
          <div className="flex gap-6">
            <div className="w-56 shrink-0 space-y-1">
              <h3 className="mb-3 text-sm font-medium text-muted-foreground">Roles</h3>
              {roles.map((r) => (
                <button
                  key={r.id}
                  onClick={() => setSelectedRoleId(r.id)}
                  className={`w-full rounded-lg px-3 py-2 text-left text-sm transition-colors ${selectedRoleId === r.id ? "bg-primary text-primary-foreground" : "hover:bg-muted"}`}
                >
                  {r.name}
                </button>
              ))}
            </div>
            <div className="flex-1">
              {selectedRole ? (
                <div className="space-y-4">
                    <div className="flex items-center justify-between">
                    <div>
                      <h2 className="text-lg font-medium">{selectedRole.name}</h2>
                      <p className="text-sm text-muted-foreground">{selectedRole.description}</p>
                    </div>
                    <div className="flex items-center gap-2">
                      {isProtected && (
                        <span className="text-sm text-muted-foreground">System role — cannot be edited</span>
                      )}
                      <div className="flex gap-1">
                        {Object.keys(TEMPLATES).map((tpl) => (
                          <Badge key={tpl} variant="outline" className={isProtected ? "pointer-events-none opacity-50" : "cursor-pointer"} onClick={() => applyTemplate(tpl)}>
                            {tpl}
                          </Badge>
                        ))}
                      </div>
                      <Button onClick={handleSave} disabled={saving || isProtected} className="gap-1.5">
                        {saving ? <Loader2 className="size-4 animate-spin" /> : <Save className="size-4" />}
                        Save
                      </Button>
                    </div>
                  </div>
                  <Separator />
                  <ScrollArea className="h-[calc(100vh-280px)]">
                    <div className="space-y-6 pr-4">
                      {Object.entries(grouped).map(([module, perms]) => {
                        const allChecked = perms.every((p) => rolePermissionCodes.has(p.code))
                        const someChecked = perms.some((p) => rolePermissionCodes.has(p.code))
                        return (
                          <div key={module}>
                            <div className="flex items-center gap-2 mb-2">
                              <input
                                type="checkbox"
                                checked={allChecked}
                                disabled={isProtected}
                                ref={(el) => { if (el) el.indeterminate = someChecked && !allChecked }}
                                onChange={() => toggleModule(perms, !allChecked)}
                                className="size-4"
                              />
                              <h4 className="text-sm font-medium capitalize">{module}</h4>
                            </div>
                            <div className="grid grid-cols-2 gap-1.5 sm:grid-cols-3 md:grid-cols-4">
                              {perms.map((perm) => {
                                const checked = rolePermissionCodes.has(perm.code)
                                return (
                                  <label
                                    key={perm.code}
                                    onClick={() => { if (!isProtected) togglePermission(perm.code) }}
                                    className={`flex items-center gap-2 rounded-lg border p-2 text-sm transition-colors ${isProtected ? "cursor-default opacity-60" : "cursor-pointer hover:bg-muted"} ${checked ? "border-primary bg-primary/5" : ""}`}
                                  >
                                    <div className={`flex size-4 shrink-0 items-center justify-center rounded-sm ${checked ? "bg-primary text-primary-foreground" : "border"}`}>
                                      {checked && <Check className="size-3" />}
                                    </div>
                                    <div className="min-w-0">
                                      <div className="truncate">{perm.name}</div>
                                      <div className="truncate text-xs text-muted-foreground">{perm.code}</div>
                                    </div>
                                  </label>
                                )
                              })}
                            </div>
                          </div>
                        )
                      })}
                    </div>
                  </ScrollArea>
                </div>
              ) : (
                <div className="flex h-64 items-center justify-center text-muted-foreground">
                  Select a role to view and edit permissions
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </>
  )
}
