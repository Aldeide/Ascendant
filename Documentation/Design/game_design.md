# Game Design Document: Ascendant

**Ascendant** is a persistent, massive multiplayer space logistics and warfare simulation. Drawing heavy inspiration from games like *Foxhole*, Ascendant places players in a persistent, system-spanning war where every ship, defense platform, automated turret, and round of ammunition is gathered, refined, manufactured, and transported by real players.

---

## 1. Executive Summary

* **Concept:** A persistent space warfare sandbox driven entirely by player logistics, manufacturing, and combat cooperation.
* **Camera Perspective:** 3D RTS Point-and-Click (Homeworld-style cursor navigation with 3D altitude adjustments).
* **Core Loop:** Extract resources $\rightarrow$ Refine and Manufacture $\rightarrow$ Transport and Supply $\rightarrow$ Fortify and Expand $\rightarrow$ Capture Star Systems.
* **Technology Stack:**
  - **Engine:** Unity 6
  - **Networking:** Unity Netcode for GameObjects (NGO)
  - **Physics/Simulation:** Server-Authoritative Burst Jobs (C# Job System)

---

## 2. Core Gameplay & Persistent Space Architecture

The war is waged across a persistent, interconnected network of star systems. The map is not a single instance, but a cluster of persistent systems running concurrently.

```mermaid
graph LR
    subgraph System Alpha (Frontline)
        JP1((Jump Point)) <--> S1[Defense Array]
    end
    subgraph System Beta (Logistics Hub)
        JP2((Jump Point)) <--> S2[Storage Depot]
        S2 <--> S3[Asteroid Belt]
    end
    JP1 <== Hyperlane ==> JP2
```

### 2.1 Star Systems & Jump Points
* **Jump Point Travel:** Systems are connected via hyperlanes demarcated by **Jump Points**. Players must navigate their ships to a Jump Point and initiate a jump sequence to transition their ship (and cargo) into adjacent systems.
* **Persistent Conquest:** Systems are dynamically owned by factions. A system is captured by destroying the enemy's Command Hub and replacing it with a friendly equivalent.
* **Frontline vs. Rear Systems:** Factions have secure rear systems (where raw materials are abundant) and active frontlines (where defensive structures and munitions are heavily consumed).

---

## 3. Player Ships & Customization

Instead of commanding fleets, each player controls and customizes **a single ship**. This fosters personal specialization and reinforces the need for cooperation.

```
+-------------------------------------------------------------+
|                        Modular Ship                         |
+---------------------+-----------------+---------------------+
|    Cargo Holds      |  Utility Slots  |    Weapon Mounts    |
| (Resource Transport)| (Manufacturing) |   (Pure Warship)    |
+---------------------+-----------------+---------------------+
```

### 3.1 Ship Roles & Customization
Players customize their hulls at friendly **Shipyards** using modules unlocked through the tech tree.

| Customization Role | Primary Modules | Description |
| :--- | :--- | :--- |
| **Logistics Transport** | Extended Cargo Holds, Engine Boosters | Designed to move massive quantities of raw materials and crated supplies between systems. |
| **Industrial / Assembler**| Mobile Fabricator, Resource Refiner | Can process materials and manufacture basic ammunition/supplies on-the-go in deep space. |
| **Combat Warship** | Heavy Shields, Guided Missile Racks, Point-Defense | Armed for escorting logistics convoys, patrolling jump lanes, and sieging enemy stations. |
| **Hybrid / Support** | Tractor Beams, Repair Arrays, Mining Lasers | A balanced configuration capable of extracting resources while repairing allied structures. |

---

## 4. Faction Infrastructure & Logistics

All structures in the galaxy are constructed by players. Construction requires resources (refined alloys, fuel, and components) to be transported directly to the building site.

### 4.1 Construction Categories

> [!IMPORTANT]
> All structures require regular maintenance supplies (Upkeep) stored in a nearby Storage Hub to prevent decay.

#### A. Raw Material Extraction & Logistics
* **Asteroid Mining Rig:** Automatic or player-assisted structures anchored to rich asteroid belts to extract raw ore and gas.
* **Resource Storage Hub:** Large depots that act as inventory drop-off points for logistics pilots. They distribute resources to nearby production structures.

#### B. Production & Manufacturing
* **Munitions Factory:** Manufactures ammunition crates (missiles, torpedoes, kinetic rounds, and space mines) using refined materials.
* **Component Assembler:** Produces structural sub-assemblies required to construct advanced defense networks and ship modules.

#### C. Facility Support & Fleet Infrastructure
* **Refueling / Resupply Depot:** Automates the distribution of fuel and ammo to friendly ships that dock or pass nearby.
* **Shipyard:** Allows players to dock to repair hulls, swap modules, customize ship builds, and respawn when their ship is destroyed.
* **Research Center:** Generates research points when supplied with Tech Parts, advancing the global faction technology.

#### D. Defensive Structures
* **Sensor Array:** Scans the star system and alerts the faction of incoming enemy warp signatures and ship movements.
* **Automated Missile Platform:** Medium-range defensive platform that fires tracking missiles at hostile targets.
* **Automated Point-Defense Turret:** Short-range, fast-tracking turret designed to shoot down incoming torpedoes and fighter craft.

---

## 5. Technology & Progression Trees

Progress is split between **Faction Advancement** and **Individual Specialization**.

```mermaid
graph TD
    subgraph Faction Tech (Global)
        F1[Advanced Alloys] --> F2[Cruiser Hulls]
        F1 --> F3[Heavy Torpedoes]
    end
    subgraph Personal Specialization (Player)
        P1[Logistics Focus] --> P2[Refining Speed]
        P3[Combat Focus] --> P4[Missile Reload]
    end
```

### 5.1 Faction Tech Tree (Global)
* **Collective Investment:** Players deposit "Tech Parts" (rare materials found during mining or salvage) into **Research Centers**.
* **Global Unlocks:** Once a node is fully researched, it benefits the entire faction (e.g., unlocking heavier ship classes, better manufacturing blueprints, or stronger defenses).

### 5.2 Individual Tech Trees (Specialization)
* **Experience Earned:** Players earn specialization points by performing specific roles (mining, shipping, defending, researching).
* **Role Perks:** Unlocks personal efficiency bonuses, such as faster mining speed, higher cargo capacity, reduced ship repair costs, or faster weapon lock-on speeds.

---

## 6. Global Ranking & War Contribution

To incentivize player roles that don't focus on combat (such as logistics and mining), the game features a comprehensive ranking system based on **War Contribution**.

### 6.1 War Contribution Points (WCP)
Players earn WCP for actions that help the faction win the war:
* **Logistics:** Delivering supply crates to frontline bases, refining raw ore, and transporting resources.
* **Engineering:** Constructing and repairing structures, rebuilding destroyed defenses, and fueling depots.
* **Combat:** Destroying enemy ships, capturing jump points, and defending faction structures.
* **Research:** Discovering tech parts and contributing to Research Center projects.

### 6.2 Rank Hierarchy and Branches

> [!TIP]
> Your rank displays as a badge next to your player name in chat and on the space HUD, showing other players your primary area of expertise and veteran status.

```
            [ Grand Admiral / Faction Leader ]
                          |
         +----------------+----------------+
         |                                 |
  [ Line Branch ]                 [ Support Branch ]
  - Fleet Commander               - Logistics Director
  - Captain                       - Chief Engineer
  - Lieutenant                    - Research Director
```

| Rank Level | Line Officer (Combat) | Logistics Officer (Industrial) | Science & Engineering (Support) |
| :--- | :--- | :--- | :--- |
| **Tier 4** | Admiral | Logistics Director | Research Director |
| **Tier 3** | Captain | Supply Superintendent | Chief Engineer |
| **Tier 2** | Commander | Logistics Coordinator | Senior Engineer |
| **Tier 1** | Lieutenant | Transport Officer | Field Technician |
| **Entry**  | Ensign | Quartermaster Apprentice | Cadet |
