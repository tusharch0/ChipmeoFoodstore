"use client"

import * as React from "react"
import { ImageIcon, Loader2, Upload, X } from "lucide-react"
import Image from "next/image"

import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

interface ImageUploadProps {
  value?: string | null
  onChange: (url: string | null) => void
  onUpload: (file: File) => Promise<string>
  folder?: string
  className?: string
}

export function ImageUpload({ value, onChange, onUpload, className }: ImageUploadProps) {
  const [preview, setPreview] = React.useState<string | null>(value ?? null)
  const [uploading, setUploading] = React.useState(false)
  const inputRef = React.useRef<HTMLInputElement>(null)

  React.useEffect(() => {
    setPreview(value ?? null)
  }, [value])

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    if (file.size > 5 * 1024 * 1024) {
      alert("File too large. Please select a file under 5MB.")
      return
    }

    setUploading(true)
    try {
      const url = await onUpload(file)
      setPreview(url)
      onChange(url)
    } catch (err) {
      console.error("Upload failed:", err)
    } finally {
      setUploading(false)
      if (inputRef.current) inputRef.current.value = ""
    }
  }

  const handleRemove = () => {
    setPreview(null)
    onChange(null)
  }

  return (
    <div className={cn("flex items-center gap-4", className)}>
      <div className="relative flex size-24 shrink-0 items-center justify-center overflow-hidden rounded-lg border bg-muted">
        {uploading ? (
          <Loader2 className="size-6 animate-spin text-muted-foreground" />
        ) : preview ? (
          <>
            <Image
              src={preview}
              alt="Preview"
              fill
              className="object-cover"
              unoptimized
            />
            <button
              type="button"
              onClick={handleRemove}
              className="absolute top-1 right-1 rounded-full bg-background/80 p-0.5 shadow-xs hover:bg-background"
            >
              <X className="size-3" />
            </button>
          </>
        ) : (
          <ImageIcon className="size-6 text-muted-foreground" />
        )}
      </div>
      <div>
        <input
          ref={inputRef}
          type="file"
          accept="image/*"
          className="hidden"
          onChange={handleFileSelect}
        />
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => inputRef.current?.click()}
          disabled={uploading}
          className="gap-1.5"
        >
          <Upload className="size-3.5" />
          {uploading ? "Uploading..." : "Select Image"}
        </Button>
        <p className="mt-1 text-xs text-muted-foreground">PNG, JPG, WEBP up to 5MB</p>
      </div>
    </div>
  )
}
