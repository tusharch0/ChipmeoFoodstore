"use client"

import * as React from "react"
import { AlertTriangle, Loader2, Trash2 } from "lucide-react"

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogMedia,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog"
import { Button } from "@/components/ui/button"

interface ConfirmDialogProps {
  open?: boolean
  onOpenChange?: (open: boolean) => void
  title?: string
  description?: string
  onConfirm: () => void
  loading?: boolean
  confirmLabel?: string
  variant?: "destructive" | "default"
  trigger?: React.ReactNode
}

export function ConfirmDialog({
  open,
  onOpenChange,
  title = "Confirm",
  description = "Are you sure you want to perform this action?",
  onConfirm,
  loading,
  confirmLabel = "Confirm",
  variant = "destructive",
  trigger,
}: ConfirmDialogProps) {
  const Wrapper = trigger ? AlertDialog : React.Fragment
  const wrapperProps = trigger ? { open, onOpenChange } : {}

  return (
    <AlertDialog {...wrapperProps}>
      {trigger && <AlertDialogTrigger>{trigger}</AlertDialogTrigger>}
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogMedia>
            {variant === "destructive" ? (
              <Trash2 className="text-destructive" />
            ) : (
              <AlertTriangle />
            )}
          </AlertDialogMedia>
          <AlertDialogTitle>{title}</AlertDialogTitle>
          <AlertDialogDescription>{description}</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction onClick={onConfirm} disabled={loading}>
            {loading && <Loader2 className="mr-2 size-4 animate-spin" />}
            {confirmLabel}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

export function DeleteConfirmDialog({
  open,
  onOpenChange,
  onConfirm,
  loading,
  itemName,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  onConfirm: () => void
  loading?: boolean
  itemName?: string
}) {
  return (
    <ConfirmDialog
      open={open}
      onOpenChange={onOpenChange}
      title="Delete"
      description={itemName ? `Are you sure you want to delete "${itemName}"? This action cannot be undone.` : "Are you sure you want to delete this item?"}
      confirmLabel="Delete"
      variant="destructive"
      onConfirm={onConfirm}
      loading={loading}
    />
  )
}
