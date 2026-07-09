"use client"

import { Badge } from "@/components/ui/badge"

const statusConfig: Record<string, { label: string; variant: "default" | "secondary" | "destructive" | "outline" | "ghost" | "link" }> = {
  active: { label: "Active", variant: "default" },
  inactive: { label: "Inactive", variant: "secondary" },
  pending: { label: "Pending", variant: "outline" },
  confirmed: { label: "Confirmed", variant: "default" },
  preparing: { label: "Preparing", variant: "ghost" },
  ready: { label: "Ready", variant: "default" },
  served: { label: "Served", variant: "secondary" },
  paid: { label: "Paid", variant: "default" },
  cancelled: { label: "Cancelled", variant: "destructive" },
  true: { label: "Active", variant: "default" },
  false: { label: "Inactive", variant: "secondary" },
}

interface StatusBadgeProps {
  status: string | boolean | null | undefined
  customLabels?: Record<string, string>
}

export function StatusBadge({ status, customLabels }: StatusBadgeProps) {
  const key = status === true || status === false ? String(status) : (status ?? "inactive")
  const config = statusConfig[key] ?? { label: key, variant: "outline" as const }

  return (
    <Badge variant={config.variant}>
      {customLabels?.[key] ?? config.label}
    </Badge>
  )
}
