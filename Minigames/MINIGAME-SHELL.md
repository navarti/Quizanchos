# Minigame UI shell

The host serves a shared CSS shell at `/css/minigame-shell.css` that's auto-loaded
on every minigame page **before** plugin styles. It gives every minigame the same
card chrome, button styling, pills, options, score chips, and finished-banner so
plugins don't have to redefine them — and so the platform's look stays consistent
no matter who built the plugin.

All shell rules are scoped under `#minigame-root`, so the shell never leaks into
navbar / footer / home page chrome. Every shell class also relies on the design
tokens defined in `/css/site.css` (`--gradient-aurora`, `--color-primary-*`,
`--space-*`, `--radius-*`, etc.) — those tokens are also part of the public API.

## Quickstart

Plugin lobby:

```html
<div class="minigame-card minigame-card--lobby minigame-card--narrow">
    <h2 class="minigame-title">My Game</h2>
    <p class="minigame-prose">Description of the game…</p>
    <div class="minigame-form">
        <div class="minigame-field">
            <label for="rounds">Rounds</label>
            <input type="number" id="rounds" value="5" />
        </div>
    </div>
    <button class="minigame-btn" data-start>Start</button>
</div>
```

Plugin game view:

```html
<div class="minigame-card">
    <div class="minigame-head">
        <h2 class="minigame-title">My Game</h2>
        <div class="minigame-head-meta">
            <span class="minigame-pill" data-progress>Round 1 of 5</span>
            <span class="minigame-pill minigame-pill--timer" data-timer>20s</span>
        </div>
    </div>

    <!-- game-specific stuff goes here, styled by your plugin's CSS -->
    <div class="my-game__board"></div>

    <div class="minigame-options" data-options>
        <button class="minigame-option" data-state="picked">Option A</button>
        <button class="minigame-option" data-state="correct">Option B</button>
        <button class="minigame-option" data-state="wrong">Option C</button>
    </div>

    <h3 class="minigame-section-title">Players</h3>
    <div class="minigame-scores">
        <div class="minigame-score is-me"><span>You</span><strong>3</strong></div>
        <div class="minigame-score"><span>Alice</span><strong>2</strong></div>
    </div>

    <div class="minigame-finished minigame-finished--win">You won!</div>
</div>
```

## Class contract

### Surface card
| Class | Purpose |
| --- | --- |
| `.minigame-card` | Default surface card with aurora top bar. Wrap your whole view in this. |
| `.minigame-card--lobby` | Centered, narrower padding for lobby/landing views. |
| `.minigame-card--narrow` | Cap width at 540px (forms, dialogs). |
| `.minigame-card--wide` | Cap width at 1200px (boards, multi-column layouts). |

### Typography
| Class | Purpose |
| --- | --- |
| `.minigame-title` | Aurora-gradient hero title (use on `<h2>`). |
| `.minigame-section-title` | Secondary heading (use on `<h3>`). |
| `.minigame-prose` | Centered muted body text for descriptions. |

### Header & meta
| Class | Purpose |
| --- | --- |
| `.minigame-head` | Flex row: title on left, meta chips on right. |
| `.minigame-head-meta` | Inline-flex container for chips. |
| `.minigame-pill` | Round chip — defaults to violet primary tone. |
| `.minigame-pill--timer` | Aurora gradient pill for timers; uses mono font. |
| `.minigame-pill--success` / `--warning` / `--danger` | Status-tone variants. |
| `.minigame-status` | Primary-tinted ribbon for "Your turn" / status messages. |

### Buttons
| Class | Purpose |
| --- | --- |
| `.minigame-btn` | Primary aurora-gradient button. Default for CTAs. |
| `.minigame-btn--secondary` | White surface, primary text. |
| `.minigame-btn--danger` | Red, for destructive actions. |
| `.minigame-btn--block` | Full width. |
| `.minigame-btn--sm` / `--lg` | Size variants. |

### Forms (lobby parameter forms)
| Class | Purpose |
| --- | --- |
| `.minigame-form` | Grid container for fields. Centered, max-width 360px. |
| `.minigame-field` | Single label+input pair (column flex). |

### Options (quiz / multi-choice answers)
| Class | Purpose |
| --- | --- |
| `.minigame-options` | Auto-fit grid, min 180px columns. |
| `.minigame-option` | Default light option button. |
| `.minigame-option[data-state="picked"]` | Amber — answer chosen, not yet revealed. |
| `.minigame-option[data-state="correct"]` | Green — right answer. |
| `.minigame-option[data-state="wrong"]` | Red — wrong answer. |

### Scores
| Class | Purpose |
| --- | --- |
| `.minigame-scores` | Auto-fit grid for score chips. |
| `.minigame-score` | Single player chip. Use `<span>` + `<strong>` inside. |
| `.minigame-score.is-me` | Highlights the local player with the aurora tint. |

### Finished banner
| Class | Purpose |
| --- | --- |
| `.minigame-finished` | Default end-of-game banner with aurora top bar. |
| `.minigame-finished--win` | Green tone (you won). |
| `.minigame-finished--loss` | Red tone (you lost). |
| `.minigame-finished--draw` | Amber tone (draw / no winner). |

### Misc
| Class | Purpose |
| --- | --- |
| `.minigame-actions` | Flex row of action buttons at the bottom of a card. |
| `.minigame-loader` | Tri-color aurora spinner for loading states. |

## Override rules

The shell is loaded **before** your plugin's CSS, so you can override any class
by re-declaring it under `#minigame-root` in your plugin stylesheet. Prefer to
add new classes (e.g. `.my-game__felt`) rather than fighting the shell — that
keeps your visual identity localized.

## Design tokens (always available)

These tokens come from `/css/site.css` and are usable from any plugin CSS:

```css
/* Aurora gradient (violet → pink → amber) */
var(--gradient-aurora)

/* Brand palette */
var(--color-primary-50 ... 900)    /* violet */
var(--color-accent-300 ... 700)    /* pink */
var(--color-highlight-300 ... 600) /* amber */

/* Status */
var(--color-success-100 / 500 / 600)
var(--color-warning-100 / 500 / 600)
var(--color-danger-100 / 500 / 600)
var(--color-info-500 / 600)

/* Surfaces & text */
var(--color-surface), var(--color-surface-soft), var(--color-surface-alt)
var(--color-text), var(--color-text-soft), var(--color-text-muted), var(--color-text-inverse)
var(--color-border), var(--color-border-strong), var(--color-focus-ring)

/* Typography */
var(--font-display), var(--font-sans), var(--font-mono)
var(--font-size-xs ... --font-size-5xl)

/* Spacing scale (4-base) */
var(--space-1 ... --space-20)

/* Radii & shadows */
var(--radius-sm ... --radius-3xl), var(--radius-pill)
var(--shadow-xs / sm / md / lg / xl)

/* Motion */
var(--transition-fast / base / slow)
```

## Versioning

The shell class names are a **public API**. Renaming a class is a breaking
change for third-party plugins. Add new classes freely, but never rename or
remove existing ones without a migration window.

The CSS file lives at:

```
Quizanchos/Quizanchos.WebApi/wwwroot/css/minigame-shell.css
```

It's served verbatim at `/css/minigame-shell.css` and `asp-append-version` adds
a cache-buster on each deploy.
