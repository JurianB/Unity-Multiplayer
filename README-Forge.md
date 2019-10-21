# Forge Networking Setup
In this guide we will take you through all the steps to install Forge and add multiplayer to our tank game.

## Step 1:
- Open the Unity Asset Store by pressing CTRL + 9 or CMD + 9;
- Type in: “Forge Networking”
![Search Result](./Images/Forge/Search-result.png)


- Download and install all the assets
![Search Result](./Images/Forge/Import.png)

## Step 2:
Open the “Network Contract Wizard” by going to *Window -> Forge Networking -> Network Contract Wizard* or bij pressing *CTRL + G or CMD + G*

## Step 3:
Click create to create a new *Network Contract*. This Network Contract is basically a set of rules in which the data will go over the network.

![Search Result](./Images/Forge/Network-contract-wizard.png)

## Step 4:
Name the new contract **PlayerMove**. Add a field to the fields section and name it **Position** which is of type **Vector3**.  

Hit the checkbox **Interpolation**, this tells Forge Networking that the position needs to smoothly transition between new updates. Leave the value as is.



Create a field for the rotation too, name it **Rotation**, which is of type **Quaternion**.

Hit the checkbox **Interpolation**, this tells Forge Networking that the rotation needs to smoothly transition between new updates. Leave the value as is.  

![Search Result](./Images/Forge/PlayerMove-Settings.png)

## Step 5
Click **Save & Compile**, this will create quite a few scripts which you can use later in the code;

The main thing you will use is the **NetworkObject**. All the new values will be asigned to this object.

## Step 6:
Open the **PlayerTankController.cs**. We no longer need to derive from **MonoBehaviour.cs**. Change the class to **PlayerMoveBehaviour**. This will 'inject' the networkObject to the **PlayerTankController** class with all the new information from all other connected clients.

## Step 7:
Make sure that you send your own position and rotation over the network by adding the following code.






