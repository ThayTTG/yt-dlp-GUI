"""Gera os ícones e o logo do YtDlpGui (tema escuro roxo, família NokyLabs).

Desenha em 4x e reduz com LANCZOS para bordas suaves.
Uso: python3 gerar_icones.py <pasta_assets> <caminho_icon_ico>
"""
import math
import sys
from PIL import Image, ImageDraw, ImageFilter

S = 4                 # supersampling
N = 48                # tamanho final dos ícones de linha
C = N * S             # tamanho da tela de desenho
W = int(2.0 * S * 48 / 24)  # espessura de traço ≈ 2px num ícone de 24

CORES = {
    "muted":  (164, 155, 184, 255),
    "branco": (245, 243, 250, 255),
    "roxo":   (178, 148, 255, 255),
    "rosa":   (251, 113, 133, 255),
}


def p(x, y):
    """Converte coordenadas da grade 24x24 para a tela."""
    k = C / 24
    return (x * k, y * k)


def r(v):
    return v * C / 24


def nova():
    im = Image.new("RGBA", (C, C), (0, 0, 0, 0))
    return im, ImageDraw.Draw(im)


def linha(d, pts, cor, w=W):
    d.line([p(*q) for q in pts], fill=cor, width=w, joint="curve")
    for q in (pts[0], pts[-1]):
        x, y = p(*q)
        d.ellipse([x - w / 2, y - w / 2, x + w / 2, y + w / 2], fill=cor)


def arco(d, cx, cy, raio, a0, a1, cor, w=W):
    x, y = p(cx, cy)
    rr = r(raio)
    d.arc([x - rr, y - rr, x + rr, y + rr], a0, a1, fill=cor, width=w)
    for a in (a0, a1):
        ax = x + rr * math.cos(math.radians(a))
        ay = y + rr * math.sin(math.radians(a))
        d.ellipse([ax - w / 2 + 1, ay - w / 2 + 1, ax + w / 2 - 1, ay + w / 2 - 1], fill=cor)


def retang(d, x0, y0, x1, y1, raio, cor, w=W, preencher=False):
    box = [*p(x0, y0), *p(x1, y1)]
    if preencher:
        d.rounded_rectangle(box, radius=r(raio), fill=cor)
    else:
        d.rounded_rectangle(box, radius=r(raio), outline=cor, width=w)


def finalizar(im, tamanho=N):
    return im.resize((tamanho, tamanho), Image.LANCZOS)


# ---------------------------------------------------------------- ícones
def ic_home(cor):
    im, d = nova()
    linha(d, [(3.5, 10.5), (12, 3.8), (20.5, 10.5)], cor)
    linha(d, [(5.5, 9), (5.5, 19.2), (18.5, 19.2), (18.5, 9)], cor)
    linha(d, [(10, 19.2), (10, 14.2), (14, 14.2), (14, 19.2)], cor)
    return im


def ic_historico(cor):
    im, d = nova()
    arco(d, 12.5, 12, 8.2, 200, 500, cor)        # círculo aberto
    linha(d, [(2.6, 6.8), (4.6, 10.6), (8.4, 9.0)], cor)  # ponta da seta
    linha(d, [(12.5, 7.6), (12.5, 12.2), (15.6, 14.2)], cor)  # ponteiros
    return im


def ic_pasta(cor):
    im, d = nova()
    linha(d, [(3, 7.2), (3, 5.8), (4.2, 4.6), (9, 4.6), (11, 6.8), (19.8, 6.8),
              (21, 8), (21, 18), (19.8, 19.2), (4.2, 19.2), (3, 18), (3, 7.2)], cor)
    linha(d, [(3, 9.6), (21, 9.6)], cor)
    return im


def ic_config(cor):
    im, d = nova()
    cx, cy = p(12, 12)
    dentes = 8
    ext, inn = r(9.2), r(7.0)
    pts = []
    for i in range(dentes * 4):
        ang = 2 * math.pi * i / (dentes * 4) - math.pi / 2
        rr = ext if (i % 4) in (1, 2) else inn
        pts.append((cx + rr * math.cos(ang), cy + rr * math.sin(ang)))
    pts.append(pts[0])
    d.line(pts, fill=cor, width=W, joint="curve")
    rr = r(3.0)
    d.ellipse([cx - rr, cy - rr, cx + rr, cy + rr], outline=cor, width=W)
    return im


def ic_link(cor):
    im, d = nova()
    # dois elos na horizontal, depois gira 45°
    retang(d, 2.5, 8.6, 13.2, 15.4, 3.4, cor)
    retang(d, 10.8, 8.6, 21.5, 15.4, 3.4, cor)
    # "apaga" o trecho do meio para o elo parecer encaixado
    d.rectangle([*p(10.8 + 0.2, 8.0), *p(13.2 - 0.2, 10.0)], fill=(0, 0, 0, 0))
    linha(d, [(8.5, 12), (15.5, 12)], cor)
    return im.rotate(45, resample=Image.BICUBIC)


def ic_video(cor):
    im, d = nova()
    retang(d, 2.8, 5, 21.2, 19, 3.2, cor)
    d.polygon([p(10, 8.8), p(10, 15.2), p(15.4, 12)], fill=cor)
    return im


def ic_musica(cor):
    im, d = nova()
    linha(d, [(9, 17), (9, 5.2), (19.5, 3.6), (19.5, 15.4)], cor)
    for cx, cy in ((6.6, 17.2), (17.1, 15.6)):
        x, y = p(cx, cy)
        rr = r(2.7)
        d.ellipse([x - rr, y - rr, x + rr, y + rr], fill=cor)
    return im


def ic_lixeira(cor):
    im, d = nova()
    linha(d, [(3.8, 6.4), (20.2, 6.4)], cor)
    linha(d, [(9, 6.4), (9, 4.2), (15, 4.2), (15, 6.4)], cor)
    linha(d, [(5.8, 6.4), (6.8, 19.4), (17.2, 19.4), (18.2, 6.4)], cor)
    linha(d, [(10.2, 10.2), (10.2, 15.8)], cor)
    linha(d, [(13.8, 10.2), (13.8, 15.8)], cor)
    return im


def ic_abrir(cor):
    im, d = nova()
    linha(d, [(11, 4.5), (5.5, 4.5), (4, 6), (4, 18.5), (5.5, 20), (18, 20), (19.5, 18.5), (19.5, 13)], cor)
    linha(d, [(14, 4), (20, 4), (20, 10)], cor)
    linha(d, [(20, 4), (11.5, 12.5)], cor)
    return im


def ic_atualizar(cor):
    im, d = nova()
    raio = 7.8
    arco(d, 12, 12, raio, 20, 300, cor)
    a = math.radians(300)
    ex, ey = 12 + raio * math.cos(a), 12 + raio * math.sin(a)
    tx, ty = -math.sin(a), math.cos(a)          # direção do movimento (horário)
    pontas = []
    for g in (40, -40):
        gr = math.radians(g)
        rx = tx * math.cos(gr) - ty * math.sin(gr)
        ry = tx * math.sin(gr) + ty * math.cos(gr)
        pontas.append((ex - 3.6 * rx, ey - 3.6 * ry))
    linha(d, [pontas[0], (ex, ey), pontas[1]], cor)
    return im


def ic_cookie(cor):
    im, d = nova()
    arco(d, 12, 12, 8.6, 0, 300, cor)
    linha(d, [(16.3, 4.6), (17.2, 7.0), (20.4, 7.6)], cor)
    for cx, cy in ((8.6, 9.6), (13.4, 13.8), (8.8, 15.4), (15.8, 17.6)):
        x, y = p(cx, cy)
        rr = r(1.15)
        d.ellipse([x - rr, y - rr, x + rr, y + rr], fill=cor)
    return im


def ic_lista(cor):
    im, d = nova()
    for y in (6.5, 12, 17.5):
        linha(d, [(9, y), (20.5, y)], cor)
        x, yy = p(4.6, y)
        rr = r(1.3)
        d.ellipse([x - rr, yy - rr, x + rr, yy + rr], fill=cor)
    return im


ICONES = {
    "home": ic_home, "historico": ic_historico, "pasta": ic_pasta, "config": ic_config,
    "link": ic_link, "video": ic_video, "musica": ic_musica, "lixeira": ic_lixeira,
    "abrir": ic_abrir, "atualizar": ic_atualizar, "cookie": ic_cookie, "lista": ic_lista,
}


# ---------------------------------------------------------------- logo
def gradiente(tam, c1, c2):
    """Gradiente diagonal de c1 (topo-esquerda) para c2 (base-direita)."""
    g = Image.new("RGBA", (tam, tam))
    px = g.load()
    for y in range(tam):
        for x in range(tam):
            t = (x + y) / (2 * (tam - 1))
            px[x, y] = tuple(int(c1[i] + (c2[i] - c1[i]) * t) for i in range(3)) + (255,)
    return g


def logo(tam=256):
    k = 4
    T = tam * k
    base = gradiente(256, (124, 77, 255), (217, 70, 239)).resize((T, T), Image.BICUBIC)
    mascara = Image.new("L", (T, T), 0)
    ImageDraw.Draw(mascara).rounded_rectangle([0, 0, T - 1, T - 1], radius=int(T * 0.26), fill=255)
    im = Image.new("RGBA", (T, T), (0, 0, 0, 0))
    im.paste(base, (0, 0), mascara)

    # brilho suave no topo
    brilho = Image.new("RGBA", (T, T), (0, 0, 0, 0))
    ImageDraw.Draw(brilho).ellipse([-T * 0.3, -T * 0.75, T * 1.3, T * 0.42], fill=(255, 255, 255, 38))
    brilho_m = Image.new("L", (T, T), 0)
    brilho_m.paste(mascara, (0, 0))
    im = Image.alpha_composite(im, Image.composite(brilho, Image.new("RGBA", (T, T)), brilho_m))

    d = ImageDraw.Draw(im)
    branco = (255, 255, 255, 255)
    u = T / 24
    # seta de download estilizada com "play" embutido
    # triângulo de play
    d.polygon([(9.9 * u, 5.8 * u), (9.9 * u, 13.2 * u), (16.1 * u, 9.5 * u)], fill=branco)
    # barra de "salvo" embaixo
    w = int(2.1 * u)
    d.line([(6.2 * u, 17.6 * u), (17.8 * u, 17.6 * u)], fill=branco, width=w)
    for x in (6.2 * u, 17.8 * u):
        d.ellipse([x - w / 2, 17.6 * u - w / 2, x + w / 2, 17.6 * u + w / 2], fill=branco)
    return im.resize((tam, tam), Image.LANCZOS)


def ilustracao_pasta(tam=192):
    k = 4
    T = tam * k
    u = T / 24
    im = Image.new("RGBA", (T, T), (0, 0, 0, 0))
    # sombra
    sombra = Image.new("RGBA", (T, T), (0, 0, 0, 0))
    ImageDraw.Draw(sombra).rounded_rectangle([3 * u, 8.6 * u, 21 * u, 21.4 * u], radius=2.2 * u, fill=(80, 40, 160, 110))
    sombra = sombra.filter(ImageFilter.GaussianBlur(T / 30))
    im = Image.alpha_composite(im, sombra)
    d = ImageDraw.Draw(im)
    # aba de trás
    d.rounded_rectangle([2.5 * u, 4.2 * u, 11.5 * u, 9 * u], radius=1.6 * u, fill=(109, 64, 217, 255))
    d.rounded_rectangle([2.5 * u, 6.2 * u, 21.5 * u, 19.5 * u], radius=2 * u, fill=(109, 64, 217, 255))
    # papel
    d.rounded_rectangle([4.5 * u, 7.4 * u, 19.5 * u, 15 * u], radius=1 * u, fill=(237, 233, 246, 255))
    # frente com gradiente
    frente = gradiente(256, (139, 92, 246), (217, 70, 239)).resize((T, T), Image.BICUBIC)
    m = Image.new("L", (T, T), 0)
    ImageDraw.Draw(m).rounded_rectangle([2.5 * u, 9.6 * u, 21.5 * u, 19.5 * u], radius=2 * u, fill=255)
    im.paste(frente, (0, 0), m)
    # seta de download na frente
    d = ImageDraw.Draw(im)
    w = int(1.5 * u)
    branco = (255, 255, 255, 240)
    d.line([(12 * u, 11.6 * u), (12 * u, 16.4 * u)], fill=branco, width=w)
    d.line([(9.8 * u, 14.4 * u), (12 * u, 16.6 * u), (14.2 * u, 14.4 * u)], fill=branco, width=w, joint="curve")
    for x, y in ((12 * u, 11.6 * u), (9.8 * u, 14.4 * u), (14.2 * u, 14.4 * u)):
        d.ellipse([x - w / 2, y - w / 2, x + w / 2, y + w / 2], fill=branco)
    return im.resize((tam, tam), Image.LANCZOS)


def main():
    pasta = sys.argv[1]
    caminho_ico = sys.argv[2]
    for nome, fn in ICONES.items():
        for nome_cor, cor in CORES.items():
            finalizar(fn(cor)).save(f"{pasta}/ic_{nome}_{nome_cor}.png", optimize=True)
    lg = logo(256)
    lg.save(f"{pasta}/logo_app.png", optimize=True)
    ilustracao_pasta(192).save(f"{pasta}/ilustracao_pasta.png", optimize=True)
    lg.save(caminho_ico, sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)])


if __name__ == "__main__":
    main()
