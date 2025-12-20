import { Router } from "express"

console.log("Server fährt hoch...")

const PORT = process.env.PORT || 9865
const VIDEO_PATH = process.env.VIDEO_PATH || '/video'
const MUSIC_PATH = process.env.MUSIC_PATH || '/music'
const RELATIVE_URL = process.env.RELATIVE_URL || '/media'

console.log("VIDEO_PATH", VIDEO_PATH)
console.log("MUSIC_PATH", MUSIC_PATH)
console.log("RELATIVE_URL", RELATIVE_URL)

const router = Router()