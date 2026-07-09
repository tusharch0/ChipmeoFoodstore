"use client"

import * as React from "react"
import { useEditor, EditorContent } from "@tiptap/react"
import StarterKit from "@tiptap/starter-kit"
import Image from "@tiptap/extension-image"
import LinkExtension from "@tiptap/extension-link"
import Placeholder from "@tiptap/extension-placeholder"
import TextAlign from "@tiptap/extension-text-align"
import Highlight from "@tiptap/extension-highlight"
import { Bold, Italic, Underline as UnderlineIcon, Strikethrough, Code, List, ListOrdered, Quote, Heading1, Heading2, Heading3, Undo, Redo, ImageIcon, LinkIcon, AlignLeft, AlignCenter, AlignRight, Highlighter, Upload } from "lucide-react"
import { Toggle } from "@/components/ui/toggle"
import { Separator } from "@/components/ui/separator"
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

interface TiptapProps {
  content: string
  onChange: (html: string) => void
  placeholder?: string
}

export function Tiptap({ content, onChange, placeholder }: TiptapProps) {
  const [mounted, setMounted] = React.useState(false)
  const [imageDialogOpen, setImageDialogOpen] = React.useState(false)
  const [imageUrl, setImageUrl] = React.useState("")
  const [uploading, setUploading] = React.useState(false)
  const [linkDialogOpen, setLinkDialogOpen] = React.useState(false)
  const [linkUrl, setLinkUrl] = React.useState("")

  React.useEffect(() => setMounted(true), [])

  const editor = useEditor({
    extensions: [
      StarterKit.configure({ heading: { levels: [1, 2, 3] }, link: false }),
      Image.configure({ inline: false }),
      LinkExtension.configure({ openOnClick: false }),
      Placeholder.configure({ placeholder: placeholder ?? "Start writing content..." }),
      TextAlign.configure({ types: ["heading", "paragraph"] }),
      Highlight,
    ],
    content,
    immediatelyRender: false,
    onUpdate: ({ editor }) => onChange(editor.getHTML()),
  })

  const handleUploadImage = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file || !editor) return
    setUploading(true)
    try {
      const { mediaService } = await import("@/lib/services/media-service")
      const result = await mediaService.upload(file, "blog")
      editor.chain().focus().setImage({ src: result.fileUrl }).run()
      setImageDialogOpen(false)
    } catch {
      alert("Upload failed")
    } finally {
      setUploading(false)
      e.target.value = ""
    }
  }

  const handleInsertImageUrl = () => {
    if (!imageUrl.trim() || !editor) return
    editor.chain().focus().setImage({ src: imageUrl.trim() }).run()
    setImageUrl("")
    setImageDialogOpen(false)
  }

  const handleInsertLink = () => {
    if (!linkUrl.trim() || !editor) return
    const href = linkUrl.trim().startsWith("http") ? linkUrl.trim() : `https://${linkUrl.trim()}`
    editor.chain().focus().setLink({ href }).run()
    setLinkUrl("")
    setLinkDialogOpen(false)
  }

  if (!mounted || !editor) return <div className="border rounded-lg p-4 min-h-[300px]"><p className="text-muted-foreground text-sm">Loading editor...</p></div>

  const ToolbarButton = ({ onClick, pressed, children }: { onClick: () => void; pressed: boolean; children: React.ReactNode }) => (
    <Toggle pressed={pressed} onPressedChange={onClick} size="sm">{children}</Toggle>
  )

  return (
    <div className="border rounded-lg overflow-hidden">
      <div className="flex flex-wrap items-center gap-0.5 p-1.5 border-b bg-muted/30">
        <ToolbarButton onClick={() => editor.chain().focus().toggleBold().run()} pressed={editor.isActive("bold")}><Bold className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().toggleItalic().run()} pressed={editor.isActive("italic")}><Italic className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().toggleUnderline().run()} pressed={editor.isActive("underline")}><UnderlineIcon className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().toggleStrike().run()} pressed={editor.isActive("strike")}><Strikethrough className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().toggleCode().run()} pressed={editor.isActive("code")}><Code className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().toggleHighlight().run()} pressed={editor.isActive("highlight")}><Highlighter className="size-4" /></ToolbarButton>

        <Separator orientation="vertical" className="mx-1 h-6" />

        <ToolbarButton onClick={() => editor.chain().focus().toggleHeading({ level: 1 }).run()} pressed={editor.isActive("heading", { level: 1 })}><Heading1 className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()} pressed={editor.isActive("heading", { level: 2 })}><Heading2 className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().toggleHeading({ level: 3 }).run()} pressed={editor.isActive("heading", { level: 3 })}><Heading3 className="size-4" /></ToolbarButton>

        <Separator orientation="vertical" className="mx-1 h-6" />

        <ToolbarButton onClick={() => editor.chain().focus().toggleBulletList().run()} pressed={editor.isActive("bulletList")}><List className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().toggleOrderedList().run()} pressed={editor.isActive("orderedList")}><ListOrdered className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().toggleBlockquote().run()} pressed={editor.isActive("blockquote")}><Quote className="size-4" /></ToolbarButton>

        <Separator orientation="vertical" className="mx-1 h-6" />

        <ToolbarButton onClick={() => editor.chain().focus().setTextAlign("left").run()} pressed={editor.isActive({ textAlign: "left" })}><AlignLeft className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().setTextAlign("center").run()} pressed={editor.isActive({ textAlign: "center" })}><AlignCenter className="size-4" /></ToolbarButton>
        <ToolbarButton onClick={() => editor.chain().focus().setTextAlign("right").run()} pressed={editor.isActive({ textAlign: "right" })}><AlignRight className="size-4" /></ToolbarButton>

        <Separator orientation="vertical" className="mx-1 h-6" />

        <Dialog open={imageDialogOpen} onOpenChange={(v) => { setImageDialogOpen(v); if (!v) setImageUrl("") }}>
          <Button variant="ghost" size="sm" onClick={() => setImageDialogOpen(true)}>
            <ImageIcon className="size-4" />
          </Button>
          <DialogContent className="sm:max-w-md">
            <DialogHeader><DialogTitle>Add Image</DialogTitle></DialogHeader>
            <div className="grid gap-4">
              <div className="grid gap-2">
                <Label>Upload from computer</Label>
                <div className="flex items-center gap-2">
                  <Button variant="outline" className="relative" disabled={uploading}>
                    <Upload className="size-4 mr-1" />{uploading ? "Uploading..." : "Choose File"}
                    <input type="file" accept="image/*" className="absolute inset-0 opacity-0 cursor-pointer" onChange={handleUploadImage} disabled={uploading} />
                  </Button>
                </div>
              </div>
              <div className="relative">
                <div className="absolute inset-0 flex items-center"><Separator className="w-full" /></div>
                <div className="relative flex justify-center"><span className="bg-background px-2 text-xs text-muted-foreground">or enter URL</span></div>
              </div>
              <div className="grid gap-2">
                <Label>Image URL</Label>
                <div className="flex gap-2">
                  <Input value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} placeholder="https://..." />
                  <Button onClick={handleInsertImageUrl} disabled={!imageUrl.trim()}>Add</Button>
                </div>
              </div>
            </div>
          </DialogContent>
        </Dialog>

        <Dialog open={linkDialogOpen} onOpenChange={(v) => { setLinkDialogOpen(v); if (!v) setLinkUrl("") }}>
          <Button variant="ghost" size="sm" onClick={() => setLinkDialogOpen(true)}>
            <LinkIcon className="size-4" />
          </Button>
          <DialogContent className="sm:max-w-md">
            <DialogHeader><DialogTitle>Add Link</DialogTitle></DialogHeader>
            <div className="flex gap-2">
              <Input value={linkUrl} onChange={(e) => setLinkUrl(e.target.value)} placeholder="https://..." />
              <Button onClick={handleInsertLink} disabled={!linkUrl.trim()}>Add</Button>
            </div>
          </DialogContent>
        </Dialog>

        <div className="ml-auto flex gap-0.5">
          <Button variant="ghost" size="sm" onClick={() => editor.chain().focus().undo().run()} disabled={!editor.can().undo()}><Undo className="size-4" /></Button>
          <Button variant="ghost" size="sm" onClick={() => editor.chain().focus().redo().run()} disabled={!editor.can().redo()}><Redo className="size-4" /></Button>
        </div>
      </div>
      <EditorContent editor={editor} className="prose prose-sm max-w-none p-4 min-h-[400px] focus:outline-none [&_.ProseMirror]:outline-none [&_.ProseMirror]:min-h-[400px]" />
    </div>
  )
}