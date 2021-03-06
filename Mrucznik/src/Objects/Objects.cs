using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Mruv.Objects;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;

namespace Mrucznik.Objects
{
    public class Objects
    {
        public List<RemovedBuilding> RemovedBuildings { private set; get; }
        public static readonly Dictionary<int, MruDynamicObject> ObjectsIDs = new Dictionary<int, MruDynamicObject>();
        public static Dictionary<int, ObjectModel> ObjectModels = new Dictionary<int, ObjectModel>();
        public static Dictionary<string, Dictionary<int, ObjectModel>> ObjectModelsCategory = new Dictionary<string, Dictionary<int, ObjectModel>>();
        public static Dictionary<string, Dictionary<int, ObjectModel>> ObjectModelsTag = new Dictionary<string, Dictionary<int, ObjectModel>>();
        
        public Objects()
        {
            try
            {
                LoadModels().Wait();
                LoadObjects();
                LoadRemoveBuidings();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void RemoveBuildingsForPlayer(BasePlayer player)
        {
            foreach (var b in RemovedBuildings)
            {
                GlobalObject.Remove(null, (int) b.Model, new Vector3(b.X, b.Y, b.Z), b.Radius);
            }
        }

        private async Task LoadModels()
        {
            Console.Write("Loading object models...");
            var request = new FetchAllModelsRequest() {ChunkSize = 1000};
            using var call = MruV.Models.FetchAllModels(request);
            while (true)
            {
                var next = call.ResponseStream.MoveNext();
                next.Wait();
                if (next.Result == false)
                {
                    break;
                }

                Console.Write(".");
                foreach (var objectModel in call.ResponseStream.Current.Models)
                {
                    ObjectModels.TryAdd(objectModel.Key, objectModel.Value);
                    if (ObjectModelsCategory.ContainsKey(objectModel.Value.Category))
                    {
                        ObjectModelsCategory[objectModel.Value.Category].TryAdd(objectModel.Key, objectModel.Value);
                    }
                    else
                    {
                        ObjectModelsCategory.Add(objectModel.Value.Category, new Dictionary<int, ObjectModel>()
                        {
                            {objectModel.Key, objectModel.Value}
                        });
                    }
                    foreach (var objectModelTag in objectModel.Value.Tags)
                    {
                        if (ObjectModelsTag.ContainsKey(objectModelTag))
                        {
                            ObjectModelsTag[objectModelTag].TryAdd(objectModel.Key, objectModel.Value);
                        }
                        else
                        {
                            ObjectModelsTag.Add(objectModelTag, new Dictionary<int, ObjectModel>()
                            {
                                {objectModel.Key, objectModel.Value}
                            });
                        }
                    }
                }
            }
            Console.WriteLine("\nModels loaded.");
        }
        
        private void LoadRemoveBuidings()
        {
            Console.Write("Loading removed buildings...");
            var request = new GetRemovedBuildingsRequest() { };
            var result = MruV.Objects.GetRemovedBuildings(request);
            RemovedBuildings = new List<RemovedBuilding>(result.RemovedBuildings);
            Console.WriteLine("\nBuildings removed.");
        }

        private void LoadObjects()
        {
            Console.Write("Loading objects..");
            var request = new FetchAllObjectsRequest {ChunkSize = 10000};
            using(var call = MruV.Objects.FetchAllObjects(request))
            {
                while (true)
                {
                    var next = call.ResponseStream.MoveNext();
                    next.Wait();
                    if (next.Result == false)
                    {
                        break;
                    }

                    Console.Write(".");
                    foreach (var currentObject in call.ResponseStream.Current.Objects)
                    {
                        var o = currentObject.Value;
                        var dynamicObject = new MruDynamicObject((uint)currentObject.Key, (int) o.Model,
                            new Vector3(o.X, o.Y, o.Z),
                            new Vector3(o.Rx, o.Ry, o.Rz),
                            o.WorldId, o.InteriorId, null, o.StreamDistance, o.DrawDistance,
                            null, o.Priority, o.EstateId); // TODO: o.PlayerId i o.AreaId

                        foreach (var material in o.Materials)
                        {
                            dynamicObject.SetMaterial((int) material.Key, material.Value.ModelId,
                                material.Value.TxdName, material.Value.TextureName, material.Value.MaterialColor);
                        }

                        foreach (var materialText in o.MaterialTexts)
                        {
                            var mt = materialText.Value;
                            dynamicObject.SetMaterialText((int) materialText.Key, mt.Text,
                                (ObjectMaterialSize) mt.MaterialSize,
                                mt.FontFace, (int) mt.FontSize, mt.Bold, mt.FontColor, mt.BackColor,
                                (ObjectMaterialTextAlign) mt.TextAlignment);
                        }

                        ObjectsIDs[currentObject.Key] = dynamicObject;
                    }
                }
            }
            Console.WriteLine("\nObjects loaded.");
        }
    }
}