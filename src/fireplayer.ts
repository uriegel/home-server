import { Request, Response } from "express"
import { readdir, stat } from "fs/promises"
import path from "path"
import { NextFunction } from "connect"

export async function serveFile(directory: string, req: Request<{ splat?: string[] }>, res: Response<any, Record<any, string>>, next: NextFunction) {
    const filePath = path.join(directory, ...(req.params.splat ? req.params.splat : []))
    if (await isDirectory(filePath)) {

        const items = await readdir(filePath, {
            withFileTypes: true
        })
        const [dirs, files] = items.partition(n => n.isDirectory())
        res.json({
            directories: dirs.map(n => n.name),
            files: files.map(n => n.name)
        })
    } else
        return next()
}

export async function isDirectory(path: string) {
    return (await stat(path)).isDirectory()
}

// export function imageResizeMiddleware(rootDir: string) {
//     return async function (req: Request, res: Response, next: NextFunction) {
//         const relPath = decodeURIComponent(req.path).replace(/^\/+/, "");
//         const filePath = path.join(rootDir, relPath);

//         const ext = path.extname(filePath).toLowerCase();
//         if (![".jpg", ".jpeg", ".png"].includes(ext)) {
//             return next();
//         }

//         const width = Number(req.query.w) || 1920
//         const height = Number(req.query.h) || 1080
//         if (!width && !height) {
//             return next();
//         }

//         if (!filePath.startsWith(rootDir)) {
//             return res.sendStatus(403);
//         }

//         try {
//             res.type("image/jpeg");

//             sharp(filePath)
//                 .resize(width || null, height || null, {
//                     fit: "inside",
//                     withoutEnlargement: true
//                 })
//                 .jpeg({
//                     quality: 90,
//                     progressive: true
//                 })
//                 .pipe(res);
//         } catch (err) {
//             return next(err);
//         }
//     };
// }
