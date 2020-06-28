[![Community Forum](https://img.shields.io/badge/Community-Forum-41BDF5.svg?style=popout)](https://community.home-assistant.io/t/home-assistant-taskbar-menu-for-windows)
[![buymeacoffee_badge](https://img.shields.io/badge/Donate-buymeacoffe-ff813f?style=flat)](https://www.buymeacoffee.com/PiotrMachowski)
![GitHub All Releases](https://img.shields.io/github/downloads/Piotrmachowski/Home-Assistant-Taskbar-Menu/total)

# Home Assistant Taskbar Menu

This application is a simple [Home Assistant](https://www.home-assistant.io/) client for Windows.
It can control entities from [supported domains](#application) and display persistent notifications.

![menu_3](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/menu_3.png)

![notification](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/notification.png)
![search](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/search.png)

## Installation

Download `Home.Assistant.Taskbar.Menu.exe` from the latest [release](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/releases/latest) and save it to a dedicated location (e.g. `C:\Users\username\HA Taskbar Menu\`).

## Configuration

### Connection

A configuration window will be opened at first run. Provide an URL and token, check the configuration and save it.

![auth_1](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/auth_1.png)

![auth_2](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/auth_2.png)

After successful configuration a `config.dat` file will be created and an icon in notification tray will appear.

![icon](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/icon.png)


### Application

This application can control entities from following domains:
* `automation`
* `climate`
* `cover`
* `fan`
* `input_boolean`
* `input_number`
* `input_select`
* `light`
* `lock`
* `media_player`
* `scene`
* `script`
* `switch`
* `vacuum`

By default menu contains all supported entities, ordered alphabetically by id.

![menu_1](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/menu_1.png)
![menu_2](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/menu_2.png)

To configure this list use option **Configure HA Taskbar Menu**.

![view_1](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/view_1.png)
![view_2](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/view_2.png)

To add entries to view use buttons from the first row:
* **Add Entity** - adds an entity to the root menu
  
  To use friendly name from Home Assistant leave **Name** empty.
  
  ![add_entry_1](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/add_entry_1.png)
  
* **Add Node** - adds a submenu to the root menu

  ![add_entry_2](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/add_entry_2.png)

* **Add Separator** - adds a separator to the root menu

To add entries to folder click on it with right mouse button and select an option.
To remove any entry from tree click on it with right mouse button and select **Delete**.

In the second row of buttons you can configure other application features:
* Switching between **light and dark modes** (change will be applied after application restart)  

  ![menu_4](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/menu_4.png)
  ![view_3](https://github.com/PiotrMachowski/Home-Assistant-Taskbar-Menu/raw/master/Images/view_3.png)

* Enabling and disabling **persistent notifications mirroring**

## Usage

* To open search menu start typing when mouse is over an open menu
* You can close search window by pressing [Esc]
* Connection configuration is stored as encrypted text in `config.dat`. 
You can copy it between computers to use the same connections parameters.
* View configuration is stored in `viewConfig.dat`.
You can copy it between computers to use the same view configuration.
* To start this application with windows add a shortcut to `StartUp` folder. You can open it by running command `shell:startup` in Run menu (shortcut: [WIN] + [R])


<a href="https://www.buymeacoffee.com/PiotrMachowski" target="_blank"><img src="https://bmc-cdn.nyc3.digitaloceanspaces.com/BMC-button-images/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>
