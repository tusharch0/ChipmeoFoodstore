"use client"

import { useState, useEffect, useRef } from "react"
import { useTheme } from "next-themes"
import { useAuth } from "@/lib/auth-context"
import { authService } from "@/lib/auth-service"
import {
  XIcon, UserIcon, SettingsIcon, Loader2Icon, CameraIcon,
  SunIcon, MoonIcon, MonitorIcon,
} from "lucide-react"

type Tab = "profile" | "settings"

const themes = [
  { key: "light", icon: SunIcon, label: "Light" },
  { key: "dark", icon: MoonIcon, label: "Dark" },
  { key: "system", icon: MonitorIcon, label: "System" },
] as const

export function SettingsDialog({
  tab: initialTab,
  onClose,
}: {
  tab: Tab | null
  onClose: () => void
}) {
  const { user, updateProfile } = useAuth()
  const { theme, setTheme } = useTheme()
  const [tab, setTab] = useState<Tab>("profile")
  const [saving, setSaving] = useState(false)
  const [uploading, setUploading] = useState(false)
  const fileRef = useRef<HTMLInputElement>(null)

  const [name, setName] = useState("")
  const [email, setEmail] = useState("")
  const [phone, setPhone] = useState("")
  const [avatarUrl, setAvatarUrl] = useState("")

  useEffect(() => {
    if (initialTab) setTab(initialTab)
  }, [initialTab])

  useEffect(() => {
    if (user) {
      setName(user.name)
      setEmail(user.email)
      setPhone(user.phone ?? "")
      setAvatarUrl(user.avatarUrl ?? "")
    }
  }, [user])

  const isOpen = initialTab !== null

  if (!isOpen || !user) return null

  const initials = user.name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .toUpperCase()
    .slice(0, 2)

  async function handleAvatarChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    setUploading(true)
    try {
      const url = await authService.uploadAvatar(file)
      setAvatarUrl(url)
    } catch {
      // ignore
    }
    setUploading(false)
  }

  async function handleSave() {
    setSaving(true)
    try {
      await updateProfile({ name, email, phone: phone || undefined, avatarUrl: avatarUrl || undefined })
    } catch {
      // ignore
    }
    setSaving(false)
  }

  const hasChanges =
    name !== user.name ||
    email !== user.email ||
    (phone ?? "") !== (user.phone ?? "") ||
    avatarUrl !== (user.avatarUrl ?? "")

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="fixed inset-0 bg-black/50" onClick={onClose} />
      <div className="relative z-10 flex w-full max-w-3xl h-[92vh] bg-background rounded-xl shadow-2xl overflow-hidden">
        <div className="w-56 shrink-0 border-r bg-muted/30 p-4 flex flex-col gap-1">
          <button
            onClick={() => setTab("profile")}
            data-active={tab === "profile"}
            className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground data-[active=true]:bg-accent data-[active=true]:text-accent-foreground"
          >
            <UserIcon className="size-4" />
            Profile
          </button>
          <button
            onClick={() => setTab("settings")}
            data-active={tab === "settings"}
            className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground data-[active=true]:bg-accent data-[active=true]:text-accent-foreground"
          >
            <SettingsIcon className="size-4" />
            Settings
          </button>
        </div>

        <div className="flex-1 flex flex-col min-w-0">
          <div className="flex items-center justify-between border-b px-6 py-4">
            <h2 className="text-lg font-semibold">
              {tab === "profile" ? "Profile" : "Settings"}
            </h2>
            <button
              onClick={onClose}
              className="rounded-full p-1.5 text-muted-foreground hover:bg-accent hover:text-accent-foreground transition-colors"
            >
              <XIcon className="size-5" />
            </button>
          </div>

          <div className="flex-1 overflow-y-auto p-6">
            {tab === "profile" ? (
              <div className="space-y-6 max-w-lg">
                <div className="flex items-center gap-4">
                  <div className="relative">
                    <div className="size-20 rounded-full overflow-hidden bg-muted flex items-center justify-center">
                      {avatarUrl ? (
                        <img src={avatarUrl} alt="" className="size-full object-cover" />
                      ) : (
                        <span className="text-2xl font-medium text-muted-foreground">{initials}</span>
                      )}
                    </div>
                    <button
                      type="button"
                      disabled={uploading}
                      onClick={() => fileRef.current?.click()}
                      className="absolute -bottom-1 -right-1 rounded-full bg-primary text-primary-foreground p-1.5 shadow-sm hover:bg-primary/90 transition-colors disabled:opacity-50"
                    >
                      {uploading ? <Loader2Icon className="size-3.5 animate-spin" /> : <CameraIcon className="size-3.5" />}
                    </button>
                    <input
                      ref={fileRef}
                      type="file"
                      accept="image/*"
                      className="hidden"
                      onChange={handleAvatarChange}
                    />
                  </div>
                  <div>
                    <p className="font-medium">{user.name}</p>
                    <p className="text-sm text-muted-foreground">{user.username}</p>
                  </div>
                </div>

                <div>
                  <label className="text-sm font-medium text-muted-foreground">Full Name</label>
                  <input
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    className="mt-1 flex h-9 w-full rounded-lg border bg-background px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                  />
                </div>

                <div>
                  <label className="text-sm font-medium text-muted-foreground">Email</label>
                  <input
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="mt-1 flex h-9 w-full rounded-lg border bg-background px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                  />
                </div>

                <div>
                  <label className="text-sm font-medium text-muted-foreground">Phone Number</label>
                  <input
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    className="mt-1 flex h-9 w-full rounded-lg border bg-background px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                  />
                </div>

                <div className="flex gap-3 pt-2">
                  <button
                    onClick={handleSave}
                    disabled={!hasChanges || saving}
                    className="inline-flex items-center justify-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground shadow-sm hover:bg-primary/90 transition-colors disabled:opacity-50 disabled:pointer-events-none"
                  >
                    {saving && <Loader2Icon className="size-4 animate-spin" />}
                    Save Changes
                  </button>
                  <button
                    onClick={onClose}
                    className="inline-flex items-center justify-center gap-2 rounded-lg border bg-background px-4 py-2 text-sm font-medium shadow-sm hover:bg-accent transition-colors"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <div className="space-y-8 max-w-lg">
                <div>
                  <h3 className="text-sm font-medium text-muted-foreground mb-3">Appearance</h3>
                  <div className="flex gap-3">
                    {themes.map((t) => {
                      const Icon = t.icon
                      const active = theme === t.key
                      return (
                        <button
                          key={t.key}
                          onClick={() => setTheme(t.key)}
                          data-active={active}
                          className="flex flex-col items-center gap-2 rounded-xl border-2 p-4 transition-colors hover:bg-accent data-[active=true]:border-primary data-[active=true]:bg-accent"
                        >
                          <Icon className="size-6" />
                          <span className="text-sm font-medium">{t.label}</span>
                        </button>
                      )
                    })}
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}