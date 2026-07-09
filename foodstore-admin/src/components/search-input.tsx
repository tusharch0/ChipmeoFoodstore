"use client"

import * as React from "react"
import { Search, X } from "lucide-react"

import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"

interface SearchInputProps extends Omit<React.ComponentProps<typeof Input>, "onChange"> {
  value: string
  onChange: (value: string) => void
  debounce?: number
}

export function SearchInput({ value, onChange, debounce = 300, placeholder = "Search...", ...props }: SearchInputProps) {
  const [localValue, setLocalValue] = React.useState(value)

  React.useEffect(() => {
    setLocalValue(value)
  }, [value])

  React.useEffect(() => {
    const timer = setTimeout(() => {
      if (localValue !== value) onChange(localValue)
    }, debounce)
    return () => clearTimeout(timer)
  }, [localValue, debounce, onChange, value])

  return (
    <div className="relative max-w-sm">
      <Search className="absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
      <Input
        placeholder={placeholder}
        value={localValue}
        onChange={(e) => setLocalValue(e.target.value)}
        className="pl-8 pr-8"
        {...props}
      />
      {localValue && (
        <Button
          variant="ghost"
          size="icon-sm"
          className="absolute right-1 top-1"
          onClick={() => { setLocalValue(""); onChange("") }}
        >
          <X className="size-3" />
        </Button>
      )}
    </div>
  )
}
